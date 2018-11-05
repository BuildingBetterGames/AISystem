using Unity.Entities;

namespace BBG.AISystem.Pathfinding
{
    public struct Map : IComponentData
    {
        /* NOT USED */
        public int index;
        public int x;
        public int y;
        public int z;
        public int zone;
        /************/

        public int isDirty;
    }
}