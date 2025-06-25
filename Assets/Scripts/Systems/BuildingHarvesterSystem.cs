using Unity.Burst;
using Unity.Entities;
using UnityEngine;

partial struct BuildingHarvesterSystem : ISystem {

    public void OnUpdate(ref SystemState state) {
        foreach (RefRW<BuildingHarvester> buildingHarvester in SystemAPI.Query<RefRW<BuildingHarvester>>()) {
            bool canHarvest = false;

            // Prüfe basierend auf dem HarvesterType
            if (buildingHarvester.ValueRO.harvesterType == BuildingHarvesterType.Buildable) {
                // Baubare Mine kann sofort sammeln
                canHarvest = true;
            } 
            else if (buildingHarvester.ValueRO.harvesterType == BuildingHarvesterType.Capturable) {
                // Einnehmbare Mine muss erst captured werden
                if (SystemAPI.HasComponent<CapturePoint>(buildingHarvester.ValueRO.entity)) {
                    var capturePoint = SystemAPI.GetComponent<CapturePoint>(buildingHarvester.ValueRO.entity);
                    canHarvest = capturePoint.isCaptured && capturePoint.controllingFaction == FactionType.CarraraGolems;
                }
            }

            // Sammle Ressourcen wenn erlaubt
            if (canHarvest) {
                buildingHarvester.ValueRW.harvestTimer -= SystemAPI.Time.DeltaTime;
                if (buildingHarvester.ValueRO.harvestTimer <= 0f) {
                    buildingHarvester.ValueRW.harvestTimer = buildingHarvester.ValueRW.harvestTimerMax;

                    ResourceManager.Instance.AddResourceAmount(buildingHarvester.ValueRO.resourceType, 1);
                    Debug.Log($"[BuildingHarvester] Sammle Ressource {buildingHarvester.ValueRO.resourceType}");
                }
            }
        }
    }
}