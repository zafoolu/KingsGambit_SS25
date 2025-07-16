using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BuildingCathedralAuthoring : MonoBehaviour {

    public float progressMax;

    public class Baker : Baker<BuildingCathedralAuthoring> {

        public override void Bake(BuildingCathedralAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new BuildingCathedral {
                progressMax = authoring.progressMax,
                rallyPositionOffset = new float3(15, 0, 0),
            });

            AddBuffer<SpawnUnitTypeBuffer>(entity);

            AddComponent(entity, new BuildingBarracksUnitEnqueue());
            SetComponentEnabled<BuildingBarracksUnitEnqueue>(entity, false);
        }
    }
}

public struct BuildingCathedral : IComponentData {
    public float progress;
    public float progressMax;
    public UnitTypeSO.UnitType activeUnitType;
    public float3 rallyPositionOffset;
    public bool onUnitQueueChanged;
}