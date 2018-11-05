using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace BBG.AISystem
{
    public class PickBerriesSystem : JobComponentSystem
    {
        struct AIUnits
        {
            [ReadOnly] public readonly int Length;
            [ReadOnly] public EntityArray Entities;
            [ReadOnly] public ComponentDataArray<Position> Position;
            [ReadOnly] public ComponentDataArray<PickBerriesComponent> PickBerries;
            public ComponentDataArray<AI> AI;
        }
        [Inject] AIUnits aiUnits;

        struct FoodData
        {
            [ReadOnly] public readonly int Length;
            [ReadOnly] public EntityArray Entities;
            [ReadOnly] public ComponentDataArray<Position> Position;
            [ReadOnly] public SubtractiveComponent<Dead> Dead;
            [ReadOnly] public ComponentDataArray<BerryBushComponent> BerryBush;
            public ComponentDataArray<Health> Health;
        }
        [Inject] FoodData foodData;

        struct GetWoodGoalJob : IJobParallelFor
        {
            public AIUnits aiUnits;
            public FoodData foodData;

            public void Execute(int index)
            {
                var ai = aiUnits.AI[index];
                if (ai.state == State.Perform && ai.targetIndex != -1)
                {
                    if (ai.targetIndex < foodData.Length && ai.target == foodData.Entities[ai.targetIndex] &&
                        foodData.Health[ai.targetIndex].amount > 0)
                    {
                        var berryHealth = foodData.Health[ai.targetIndex];
                        if (ai.timer < 0)
                        {
                            berryHealth.amount--;
                            ai.timer = 1f;
                            foodData.Health[ai.targetIndex] = berryHealth;
                            aiUnits.AI[index] = ai;
                        }
                    }
                    else
                    {
                        ai.targetIndex = -1;
                        aiUnits.AI[index] = ai;
                    }
                }
                else
                {
                    float dist = 99999999f;
                    float3 bushPos = new float3();
                    Entity bush = new Entity();
                    int bushIndex = -1;
                    bush.Index = -1;
                    for (int i = 0; i < foodData.Length; ++i)
                    {
                        float distance = math.distance(aiUnits.Position[index].Value, foodData.Position[i].Value);
                        if (distance < dist)
                        {
                            dist = distance;
                            bush = foodData.Entities[i];
                            bushPos = foodData.Position[i].Value;
                            bushIndex = i;
                        }
                    }
                    if (bush.Index != -1)
                    {
                        ai.target = bush;
                        ai.targetPos = bushPos;
                        ai.targetIndex = bushIndex;
                    }
                    else
                    {
                        var entity = ai.target;
                        entity.Index = -1;
                        ai.target = entity;
                        ai.targetIndex = -1;
                    }
                    aiUnits.AI[index] = ai;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new GetWoodGoalJob()
            {
                aiUnits = aiUnits,
                foodData = foodData
            };
            return job.Schedule(aiUnits.Length, 64, inputDeps);
        }
    }
}