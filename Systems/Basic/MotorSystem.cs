using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace BBG.AISystem
{
    public class MotorSystem : JobComponentSystem
    {
        [BurstCompile]
        struct MotorJob : IJobProcessComponentData<Position, Velocity>
        {
            public float dt;

            public void Execute(ref Position position, [ReadOnly]ref Velocity velocity)
            {
                position.Value += velocity.Value * dt;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new MotorJob() { dt = Time.deltaTime };
            return job.Schedule(this, inputDeps);
        }
    }
}