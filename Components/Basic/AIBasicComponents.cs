using Unity.Entities;
using Unity.Mathematics;

namespace BBG.AISystem
{
    public struct NodeDebug : IComponentData
    {
        public Entity region;
        public int index;
    }

    public struct Velocity : IComponentData
    {
        public float3 Value;
    }

    public struct AI : IComponentData
    {
        public Task previousTask;
        public Task task;
        public State state;
        public Entity target; // not really used
        public int targetIndex;
        public float3 targetPos;
        public float range;
        public float timer;
        public int regionIndex;
        public int needsPath;
        public int3 start;
        public int3 end;
    }

    public struct Health : IComponentData
    {
        public int amount;
        public int max;
    }
    public struct Dead : IComponentData { }
}