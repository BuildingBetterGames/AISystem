using Unity.Entities;
using Unity.Mathematics;

namespace BBG.AISystem.Pathfinding
{
    public struct Node : IBufferElementData
    {
        public int x;
        public int y;
        public int z;

        public int up;
        public int down;
        public int left;
        public int right;

        public int upleft;
        public int upright;
        public int downleft;
        public int downright;
    }

    public struct Path : IBufferElementData
    {
        public int3 value;
    }

    public struct Spawner : IBufferElementData
    {
        public int3 position;
    }
}