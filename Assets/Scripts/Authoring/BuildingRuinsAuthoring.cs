using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BuildingRuinsAuthoring : MonoBehaviour {

    public float progressMax;

    public class Baker : Baker<BuildingRuinsAuthoring> {

        public override void Bake(BuildingRuinsAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new BuildingRuins {
                progressMax = authoring.progressMax,
                rallyPositionOffset = new float3(15, 0, 0),
            });

            AddBuffer<SpawnUnitTypeBuffer>(entity);

            AddComponent(entity, new BuildingBarracksUnitEnqueue());
            SetComponentEnabled<BuildingBarracksUnitEnqueue>(entity, false);
        }
    }
}

public struct BuildingRuins : IComponentData {
    public float progress;
    public float progressMax;
    public UnitTypeSO.UnitType activeUnitType;
    public float3 rallyPositionOffset;
    public bool onUnitQueueChanged;
}