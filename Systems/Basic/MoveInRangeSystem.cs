using BBG.AISystem.Pathfinding;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace BBG.AISystem
{
    public class MoveInRangeSystem : JobComponentSystem
    {
        struct AIUnits
        {
            [ReadOnly] public readonly int Length;
            [ReadOnly] public EntityArray Entities;
            [ReadOnly] public ComponentDataArray<Position> Position;
            public ComponentDataArray<Velocity> Velocity;
            public ComponentDataArray<AI> AI;
            public BufferArray<Path> Path;
        }
        [Inject] AIUnits aiUnits;

        struct MoveInRangeJob : IJobParallelFor
        {
            public float Speed;
            public AIUnits aiUnits;

            public void Execute(int index)
            {
                if (aiUnits.AI[index].state == State.Move)
                {
                    float distance = math.distance(aiUnits.Position[index].Value, aiUnits.Path[index][0].value);
                    if (distance > aiUnits.AI[index].range)
                    {
                        var velocity = aiUnits.Velocity[index];
                        velocity.Value = math.normalize(aiUnits.Path[index][0].value - aiUnits.Position[index].Value) * Speed;
                        aiUnits.Velocity[index] = velocity;
                    }
                    else
                    {
                        var ai = aiUnits.AI[index];
                        var velocity = aiUnits.Velocity[index];
                        if (aiUnits.Path[index].Length > 1)
                        {
                            aiUnits.Path[index].RemoveAt(0);
                        }
                        else
                        {
                            velocity.Value = 0;
                            ai.timer = 1f;
                            ai.state = State.Perform;
                        }
                        aiUnits.AI[index] = ai;
                        aiUnits.Velocity[index] = velocity;
                    }
                }
                else
                {
                    var velocity = aiUnits.Velocity[index];
                    velocity.Value = 0;
                    aiUnits.Velocity[index] = velocity;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new MoveInRangeJob
            {
                Speed = 2f,
                aiUnits = aiUnits
            };
            return job.Schedule(aiUnits.Length, 64, inputDeps);
        }
    }
}