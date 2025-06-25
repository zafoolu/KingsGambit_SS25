using Unity.Burst;
using Unity.Entities;

public partial struct GoldConverterSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (RefRW<GoldConverter> converter in SystemAPI.Query<RefRW<GoldConverter>>())
        {
            converter.ValueRW.conversionTimer -= SystemAPI.Time.DeltaTime;
            
            if (converter.ValueRO.conversionTimer <= 0f)
            {
                converter.ValueRW.conversionTimer = converter.ValueRW.conversionTimerMax;
                
                if (ResourceManager.Instance.CanSpendResourceAmount(
                    new ResourceAmount { 
                        resourceType = ResourceTypeSO.ResourceType.Gold, 
                        amount = converter.ValueRO.goldCost 
                    }))
                {
                    ResourceManager.Instance.SpendResourceAmount(
                        new ResourceAmount { 
                            resourceType = ResourceTypeSO.ResourceType.Gold, 
                            amount = converter.ValueRO.goldCost 
                        });
                    
                    ResourceManager.Instance.AddResourceAmount(
                        ResourceTypeSO.ResourceType.Goldessence, 
                        converter.ValueRO.goldessenceGain);
                }
            }
        }
    }
}