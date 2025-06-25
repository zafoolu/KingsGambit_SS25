using Unity.Entities;
using UnityEngine;

public enum BuildingHarvesterType {
    Buildable,   // Mine die gebaut werden kann
    Capturable   // Mine die eingenommen werden muss
}

public class BuildingHarvesterAuthoring : MonoBehaviour {
    public float harvestTimerMax;
    public ResourceTypeSO.ResourceType resourceType;
    public BuildingHarvesterType harvesterType = BuildingHarvesterType.Buildable;  // Standard: Baubare Mine
    public float captureRadius = 5f;  // Standard Capture-Radius
    public float captureTime = 5f;    // Standard Zeit zum Capturen

    public class Baker : Baker<BuildingHarvesterAuthoring> {

        public override void Bake(BuildingHarvesterAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            // Füge BuildingHarvester Komponente hinzu
            AddComponent(entity, new BuildingHarvester {
                entity = entity,
                harvestTimerMax = authoring.harvestTimerMax,
                resourceType = authoring.resourceType,
                harvesterType = authoring.harvesterType,
            });

            // Füge CapturePoint Komponente hinzu
            AddComponent(entity, new CapturePoint {
                radius = authoring.captureRadius,
                timeToCapture = authoring.captureTime,
                currentCaptureTime = 0f,
                isCaptured = false,
                controllingFaction = FactionType.None
            });
        }
    }
}



public struct BuildingHarvester : IComponentData {

    public Entity entity;
    public float harvestTimer;
    public float harvestTimerMax;
    public ResourceTypeSO.ResourceType resourceType;
    public BuildingHarvesterType harvesterType;

}