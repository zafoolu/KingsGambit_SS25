using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BuildingWorkshopAuthoring : MonoBehaviour {

    public float progressMax;

    public class Baker : Baker<BuildingWorkshopAuthoring> {

        public override void Bake(BuildingWorkshopAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new BuildingWorkshop {
                progressMax = authoring.progressMax,
                rallyPositionOffset = new float3(15, 0, 0),
            });

            AddBuffer<SpawnUnitTypeBuffer>(entity);

            AddComponent(entity, new BuildingBarracksUnitEnqueue());
            SetComponentEnabled<BuildingBarracksUnitEnqueue>(entity, false);
        }
    }
}

public struct BuildingWorkshop : IComponentData {
    public float progress;
    public float progressMax;
    public UnitTypeSO.UnitType activeUnitType;
    public float3 rallyPositionOffset;
}