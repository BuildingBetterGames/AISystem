using Unity.Entities;

namespace BBG.AISystem.Pathfinding
{
    public struct Region : IComponentData
    {
        public int index;
        public int x;
        public int y;
        public int z;

        public int zone;
        public int isDirty;
    }

    public struct Connection : IBufferElementData
    {
        public int index;
        public Direction direction;
    }

    public enum Direction
    {
        North,
        East,
        South,
        West
    }
}