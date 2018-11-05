using Unity.Entities;

namespace BBG.AISystem
{
    public struct Faction : IComponentData
    {
        public int Value;
    }

    public struct TreeComponent : IComponentData { }
    public struct BerryBushComponent : IComponentData { }
}