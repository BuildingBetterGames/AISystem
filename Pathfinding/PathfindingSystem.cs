using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace BBG.AISystem.Pathfinding
{
    public class PathfindingSystem : JobComponentSystem
    {
        public static int dimension = 20;

        struct Regions
        {
            [ReadOnly] public readonly int Length;
            [ReadOnly] public EntityArray Entities;
            [ReadOnly] public ComponentDataArray<Region> Region;
            [ReadOnly] public BufferArray<Node> Node;
        }
        [Inject] Regions regions;

        struct AIUnits
        {
            [ReadOnly] public readonly int Length;
            [ReadOnly] public EntityArray Entities;
            [ReadOnly] public ComponentDataArray<Position> Position;
            public ComponentDataArray<AI> AI;
            public BufferArray<Path> Path;
        }
        [Inject] AIUnits aiUnits;

        struct Pointer
        {
            public int nodeIndex;
            public int parentIndex;
            public int gCost;
            public int hCost;
        }

        struct PathfindingJob : IJobParallelFor
        {
            public Regions regions;
            public AIUnits aiUnits;

            private int GetDistance(Node node1, Node node2)
            {
                int distanceX = Mathf.Abs(node1.x - node2.x);
                int distanceY = Mathf.Abs(node1.y - node2.y);

                if (distanceX > distanceY)
                {
                    return 14 * distanceY + 10 * (distanceX - distanceY);
                }
                return 14 * distanceX + 10 * (distanceY - distanceX);
            }

            private int getPointer(int nodeIndex, NativeList<Pointer> set)
            {
                for (int i = 0; i < set.Length; i++)
                {
                    if (set[i].nodeIndex == nodeIndex)
                    {
                        return i;
                    }
                }
                return -1;
            }

            private int getLowestCost(NativeList<Pointer> set)
            {
                int fCost = 999999999;
                int index = -1;
                for (int i = 0; i < set.Length; i++)
                {
                    if (set[i].gCost + set[i].hCost < fCost)
                    {
                        fCost = set[i].gCost + set[i].hCost;
                        index = i;
                    }
                }
                return index;
            }

            public void Execute(int index)
            {
                if (aiUnits.AI[index].needsPath == 1)
                {
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (regions.Region[i].index == aiUnits.AI[index].regionIndex)
                        {
                            // do pathfinding
                            var nodes = regions.Node[i];
                            int startIndex = (((int)aiUnits.Position[index].Value.y) * PathfindingSystem.dimension) + ((int)aiUnits.Position[index].Value.x);
                            int endIndex = (((int)aiUnits.AI[index].targetPos.y) * PathfindingSystem.dimension) + ((int)aiUnits.AI[index].targetPos.x);
                            if (startIndex >= nodes.Length || startIndex < 0 ||
                                endIndex >= nodes.Length || endIndex < 0)
                            {
                                // fail
                                return;
                            }
                            bool pathFound = false;
                            Node startNode = nodes[startIndex];
                            Node endNode = nodes[endIndex];
                            Pointer startPointer = new Pointer();
                            startPointer.nodeIndex = startIndex;
                            startPointer.parentIndex = -1;
                            startPointer.gCost = 0;
                            startPointer.hCost = GetDistance(startNode, endNode);

                            NativeList<Pointer> openSet = new NativeList<Pointer>(Allocator.Temp);
                            NativeHashMap<int, Pointer> closedSet = new NativeHashMap<int, Pointer>(PathfindingSystem.dimension * PathfindingSystem.dimension, Allocator.Temp);
                            openSet.Add(startPointer);

                            while (openSet.Length > 0)
                            {
                                int lowest = getLowestCost(openSet);
                                Pointer pointer = openSet[lowest];
                                openSet.RemoveAtSwapBack(lowest);
                                closedSet.TryAdd(pointer.nodeIndex, pointer);

                                Node node = nodes[pointer.nodeIndex];
                                if (endNode.x == node.x && endNode.y == node.y && endNode.z == node.z)
                                {
                                    // found end of path!
                                    endIndex = pointer.nodeIndex;
                                    pathFound = true;
                                    break;
                                }

                                // get neighbors
                                int[] neighbors = new int[8];
                                neighbors[0] = node.up;
                                neighbors[1] = node.down;
                                neighbors[2] = node.left;
                                neighbors[3] = node.right;
                                neighbors[4] = node.upleft;
                                neighbors[5] = node.upright;
                                neighbors[6] = node.downleft;
                                neighbors[7] = node.downright;
                                Pointer reference;
                                for (int n = 0; n < neighbors.Length; n++)
                                {
                                    if (neighbors[n] != -1)
                                    {
                                        if (!closedSet.TryGetValue(neighbors[n], out reference))
                                        {
                                            int pointerIndex = getPointer(neighbors[n], openSet);
                                            if (pointerIndex == -1)
                                            {
                                                var p = new Pointer();
                                                p.nodeIndex = neighbors[n];
                                                p.gCost = pointer.gCost + GetDistance(node, nodes[p.nodeIndex]);
                                                p.hCost = GetDistance(nodes[p.nodeIndex], endNode);
                                                p.parentIndex = pointer.nodeIndex;
                                                openSet.Add(p);
                                            }
                                            else
                                            {
                                                reference = openSet[pointerIndex];
                                                if (pointer.gCost + pointer.hCost < reference.gCost + reference.hCost)
                                                {
                                                    Node refNode = nodes[reference.nodeIndex];
                                                    reference.parentIndex = pointer.nodeIndex;
                                                    reference.gCost = pointer.gCost + GetDistance(node, refNode);
                                                    reference.hCost = GetDistance(refNode, endNode);
                                                    openSet[pointerIndex] = reference;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (pathFound)
                            {
                                NativeList<int3> waypoints = new NativeList<int3>(Allocator.Temp);
                                int retraceIndex = endIndex;
                                Pointer retrace;
                                while (retraceIndex != -1)
                                {
                                    if (closedSet.TryGetValue(retraceIndex, out retrace))
                                    {
                                        Node node = nodes[retrace.nodeIndex];
                                        waypoints.Add(new int3(node.x, node.y, node.z));
                                        retraceIndex = retrace.parentIndex;
                                    }
                                }
                                aiUnits.Path[index].Clear();
                                for (int w = waypoints.Length - 1; w >= 0; w--)
                                {
                                    aiUnits.Path[index].Add(new Path()
                                    {
                                        value = waypoints[w]
                                    });
                                }
                                var ai = aiUnits.AI[index];
                                ai.needsPath = 2;
                                aiUnits.AI[index] = ai;
                                waypoints.Dispose();
                            }

                            openSet.Dispose();
                            closedSet.Dispose();
                        }
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new PathfindingJob
            {
                regions = regions,
                aiUnits = aiUnits
            };
            return job.Schedule(aiUnits.Length, 64, inputDeps);
        }
    }
}