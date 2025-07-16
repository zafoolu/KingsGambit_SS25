using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BuildingBloodcircleAuthoring : MonoBehaviour {

    public float progressMax;

    public class Baker : Baker<BuildingBloodcircleAuthoring> {

        public override void Bake(BuildingBloodcircleAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new BuildingBloodcircle {
                progressMax = authoring.progressMax,
                rallyPositionOffset = new float3(15, 0, 0),
            });

            AddBuffer<SpawnUnitTypeBuffer>(entity);

            AddComponent(entity, new BuildingBarracksUnitEnqueue());
            SetComponentEnabled<BuildingBarracksUnitEnqueue>(entity, false);
        }
    }
}

public struct BuildingBloodcircle : IComponentData {
    public float progress;
    public float progressMax;
    public UnitTypeSO.UnitType activeUnitType;
    public float3 rallyPositionOffset;
    public bool onUnitQueueChanged;
}