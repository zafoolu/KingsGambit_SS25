using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct GameEndSystem : ISystem {


    public void OnUpdate(ref SystemState state) {
        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((
            RefRW<Health> health,
            Entity entity) 
            in SystemAPI.Query<
                RefRW<Health>>().WithEntityAccess()) {

            if (health.ValueRO.healthAmount <= 0) {
                // This entity is dead
                health.ValueRW.onDead = true;

                // Check if this is a King unit - if so, trigger Game Over
                if (SystemAPI.HasComponent<King>(entity)) {
                    // King is dead - trigger Game Over UI immediately
                    if (DOTSEventsManager.Instance != null) {
                        DOTSEventsManager.Instance.TriggerOnKingDead();
                    }
                }

                if (SystemAPI.HasComponent<BuildingConstruction>(entity)) {
                    BuildingConstruction buildingConstruction = SystemAPI.GetComponent<BuildingConstruction>(entity);
                    entityCommandBuffer.DestroyEntity(buildingConstruction.visualEntity);
                }

                entityCommandBuffer.DestroyEntity(entity);
            }
        }
    }


}