using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BBG.AISystem
{
    public class DeathSystem : JobComponentSystem
    {
        [Inject] EndFrameBarrier endFrameBarrier;

        struct HealthObjects
        {
            [ReadOnly] public readonly int Length;
            [ReadOnly] public EntityArray Entities;
            [ReadOnly] public ComponentDataArray<Health> Health;
            [ReadOnly] public SubtractiveComponent<Dead> Dead;
        }
        [Inject] HealthObjects objects;

        struct DeathJob : IJobParallelFor
        {
            public HealthObjects objects;
            [NativeDisableParallelForRestriction] public EntityCommandBuffer commandBuffer;

            public void Execute(int index)
            {
                if (objects.Health[index].amount == 0)
                {
                    commandBuffer.AddComponent(objects.Entities[index], new Dead());
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new DeathJob()
            {
                objects = objects,
                commandBuffer = endFrameBarrier.CreateCommandBuffer()
            };
            return job.Schedule(objects.Length, 64, inputDeps);
        }
    }
}