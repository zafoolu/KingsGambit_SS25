using Unity.Entities;
using UnityEngine;

public class GoldConverterAuthoring : MonoBehaviour
{
    public float conversionTimerMax = 5f;
    public int goldCost = 5;
    public int goldessenceGain = 1;

    public class Baker : Baker<GoldConverterAuthoring>
    {
        public override void Bake(GoldConverterAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new GoldConverter
            {
                conversionTimer = authoring.conversionTimerMax,
                conversionTimerMax = authoring.conversionTimerMax,
                goldCost = authoring.goldCost,
                goldessenceGain = authoring.goldessenceGain
            });
        }
    }
}

public struct GoldConverter : IComponentData
{
    public float conversionTimer;
    public float conversionTimerMax;
    public int goldCost;
    public int goldessenceGain;
}