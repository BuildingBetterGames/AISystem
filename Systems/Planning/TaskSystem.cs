using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace BBG.AISystem
{
    public class TaskSystem : JobComponentSystem
    {
        [Inject] EndFrameBarrier endFrameBarrier;

        struct AIUnits
        {
            [ReadOnly] public readonly int Length;
            [ReadOnly] public EntityArray Entities;
            public ComponentDataArray<AI> AI;
        }
        [Inject] AIUnits aiUnits;

        struct TaskJob : IJobParallelFor
        {
            public AIUnits aiUnits;
            [NativeDisableParallelForRestriction] public EntityCommandBuffer commandBuffer;

            public void Execute(int index)
            {
                var ai = aiUnits.AI[index];
                if (ai.previousTask != ai.task)
                {
                    switch (ai.previousTask)
                    {
                        case Task.None:
                            commandBuffer.RemoveComponent(aiUnits.Entities[index], typeof(IdleComponent));
                            break;
                        case Task.Explore:
                            commandBuffer.RemoveComponent(aiUnits.Entities[index], typeof(ExploreComponent));
                            break;
                        case Task.Kill:
                            commandBuffer.RemoveComponent(aiUnits.Entities[index], typeof(AttackComponent));
                            break;
                        case Task.ChopTree:
                            commandBuffer.RemoveComponent(aiUnits.Entities[index], typeof(ChopTreeComponent));
                            break;
                        case Task.PickBerries:
                            commandBuffer.RemoveComponent(aiUnits.Entities[index], typeof(PickBerriesComponent));
                            break;
                        case Task.HuntMeat:
                            commandBuffer.RemoveComponent(aiUnits.Entities[index], typeof(HuntMeatComponent));
                            break;
                    }

                    switch (ai.task)
                    {
                        case Task.None:
                            commandBuffer.AddComponent(aiUnits.Entities[index], new IdleComponent());
                            break;
                        case Task.Explore:
                            commandBuffer.AddComponent(aiUnits.Entities[index], new ExploreComponent());
                            break;
                        case Task.Kill:
                            commandBuffer.AddComponent(aiUnits.Entities[index], new AttackComponent());
                            break;
                        case Task.ChopTree:
                            commandBuffer.AddComponent(aiUnits.Entities[index], new ChopTreeComponent());
                            break;
                        case Task.PickBerries:
                            commandBuffer.AddComponent(aiUnits.Entities[index], new PickBerriesComponent());
                            break;
                        case Task.HuntMeat:
                            commandBuffer.AddComponent(aiUnits.Entities[index], new HuntMeatComponent());
                            break;
                    }

                    ai.previousTask = ai.task;
                    aiUnits.AI[index] = ai;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new TaskJob()
            {
                aiUnits = aiUnits,
                commandBuffer = endFrameBarrier.CreateCommandBuffer()
            };
            return job.Schedule(aiUnits.Length, 1000, inputDeps);///TODO: fix inner loop batch count
        }
    }
}