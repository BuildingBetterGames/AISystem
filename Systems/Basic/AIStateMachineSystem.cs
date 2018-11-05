using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace BBG.AISystem
{
    public class AIStateMachineSystem : JobComponentSystem
    {
        [BurstCompile]
        struct StateMachineJob : IJobProcessComponentData<AI>
        {
            public float deltaTime;

            public void Execute(ref AI ai)
            {
                if (ai.state == State.Idle || ai.state == State.None)
                {
                    if (ai.targetIndex == -1)
                    {
                        ai.state = State.Idle;
                    }
                    else
                    {
                        if (ai.needsPath == 0)
                        {
                            ai.needsPath = 1;
                        }
                        else if (ai.needsPath == 2)
                        {
                            ai.state = State.Move;
                        }
                        else
                        {
                            ai.state = State.Idle;
                        }
                    }
                }
                else if (ai.state == State.Perform)
                {
                    if (ai.targetIndex == -1)
                    {
                        ai.state = State.Idle;
                    }
                    else
                    {
                        ai.timer -= deltaTime;
                    }
                }
                else
                {
                    int i = 0;
                    i++;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new StateMachineJob
            {
                deltaTime = Time.deltaTime
            };
            return job.Schedule(this, inputDeps);
        }
    }
}