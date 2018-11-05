using Unity.Entities;

namespace BBG.AISystem
{
    public enum Task
    {
        None,
        Explore,
        Kill,
        ChopTree,
        PickBerries,
        HuntMeat,
        TaskCount
    }
    public enum State
    {
        None,
        Idle,
        Move,
        Perform
    }

    public struct IdleComponent : IComponentData { }
    public struct WanderComponent : IComponentData { }
    public struct PatrolComponent : IComponentData { }
    public struct ExploreComponent : IComponentData { }
    public struct AttackComponent : IComponentData { }
    public struct ChopTreeComponent : IComponentData { }
    public struct PickBerriesComponent : IComponentData { }
    public struct HuntMeatComponent : IComponentData { }
}