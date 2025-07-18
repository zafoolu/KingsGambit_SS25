

using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
partial struct SimpleSpawnerSystem : ISystem {

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        float deltaTime = SystemAPI.Time.DeltaTime;
        
        // EntityCommandBuffer für structural changes
        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((
            RefRW<SimpleSpawner> spawner,
            RefRO<LocalTransform> transform)
            in SystemAPI.Query<
                RefRW<SimpleSpawner>,
                RefRO<LocalTransform>>()) {

            if (spawner.ValueRO.prefabEntity == Entity.Null) continue;

            // Timer runterzählen
            spawner.ValueRW.timer -= deltaTime;

            // Nur spawnen wenn Timer abgelaufen ist
            if (spawner.ValueRO.timer <= 0f) {
                
                // Debug: Prüfe Target Position genauer
                float3 targetPos = spawner.ValueRO.targetPosition;
                
                // Bessere Validierung: Prüfe ob alle Koordinaten 0 sind
                bool hasValidTarget = !(math.abs(targetPos.x) < 0.001f && 
                                       math.abs(targetPos.y) < 0.001f && 
                                       math.abs(targetPos.z) < 0.001f);
                
                // Debug Output (nur im Editor)
                #if UNITY_EDITOR 
                               if (hasValidTarget) {
                    Debug.Log($"Spawning FlagBearer with VALID target: {targetPos.x}, {targetPos.y}, {targetPos.z}");
                } else {
                    Debug.LogWarning("Spawning FlagBearer with INVALID target - all coordinates are near zero!");
                    Debug.Log($"Target was: {targetPos.x}, {targetPos.y}, {targetPos.z}");
                }
                #endif
                
                // Spawne die Entities
                for (int i = 0; i < spawner.ValueRO.spawnAmount; i++) {
                    Entity newEntity = entityCommandBuffer.Instantiate(spawner.ValueRO.prefabEntity);
                    
                    // Zufällige Position um den Spawner
                    Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)(i + 1 + SystemAPI.Time.ElapsedTime * 1000));
                    float3 randomPos = new float3(
                        random.NextFloat(-spawner.ValueRO.randomOffset.x, spawner.ValueRO.randomOffset.x),
                        random.NextFloat(-spawner.ValueRO.randomOffset.y, spawner.ValueRO.randomOffset.y),
                        random.NextFloat(-spawner.ValueRO.randomOffset.z, spawner.ValueRO.randomOffset.z)
                    );
                    
                    float3 spawnPos = transform.ValueRO.Position + randomPos;
                    entityCommandBuffer.SetComponent(newEntity, LocalTransform.FromPosition(spawnPos));
                    
                    // Entferne UnitMover und SetupUnitMoverDefaultPosition Komponenten falls vorhanden
                    entityCommandBuffer.RemoveComponent<UnitMover>(newEntity);
                    entityCommandBuffer.RemoveComponent<SetupUnitMoverDefaultPosition>(newEntity);
                    
                    // Füge FlagBearer Komponente hinzu
                    entityCommandBuffer.AddComponent(newEntity, new FlagBearer {
                        formationWidth = 3,
                        formationHeight = 3,
                        unitSpacing = 2f,
                        formationDistance = 3f,
                        moveSpeed = spawner.ValueRO.moveSpeed,
                        rotationSpeed = 10f,
                        targetPosition = hasValidTarget ? targetPos : spawnPos,
                        isMoving = hasValidTarget
                    });
                    
                    #if UNITY_EDITOR
                    Debug.Log($"Added FlagBearer component - Target: {(hasValidTarget ? targetPos : spawnPos)}");
                    #endif
                }

                // Timer zurücksetzen
                spawner.ValueRW.timer = spawner.ValueRO.spawnInterval;
            }
        }
    }
}
