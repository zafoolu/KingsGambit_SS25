

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
                    Debug.Log($"Spawning Formation with VALID target: {targetPos.x}, {targetPos.y}, {targetPos.z}");
                } else {
                    Debug.LogWarning("Spawning Formation with INVALID target - all coordinates are near zero!");
                    Debug.Log($"Target was: {targetPos.x}, {targetPos.y}, {targetPos.z}");
                }
                #endif
                
                // Spawne Formationen
                for (int formationIndex = 0; formationIndex < spawner.ValueRO.spawnAmount; formationIndex++) {
                    
                    // Zufällige Position um den Spawner für diese Formation
                    Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)(formationIndex + 1 + SystemAPI.Time.ElapsedTime * 1000));
                    float3 randomPos = new float3(
                        random.NextFloat(-spawner.ValueRO.randomOffset.x, spawner.ValueRO.randomOffset.x),
                        random.NextFloat(-spawner.ValueRO.randomOffset.y, spawner.ValueRO.randomOffset.y),
                        random.NextFloat(-spawner.ValueRO.randomOffset.z, spawner.ValueRO.randomOffset.z)
                    );
                    
                    float3 formationSpawnPos = transform.ValueRO.Position + randomPos;
                    
                    // 1. Erstelle FlagBearer
                    Entity flagBearerEntity = entityCommandBuffer.Instantiate(spawner.ValueRO.prefabEntity);
                    entityCommandBuffer.SetComponent(flagBearerEntity, LocalTransform.FromPosition(formationSpawnPos));
                    
                    // Entferne UnitMover und SetupUnitMoverDefaultPosition Komponenten falls vorhanden
                    entityCommandBuffer.RemoveComponent<UnitMover>(flagBearerEntity);
                    entityCommandBuffer.RemoveComponent<SetupUnitMoverDefaultPosition>(flagBearerEntity);
                    
                    // Füge FlagBearer Komponente hinzu
                    entityCommandBuffer.AddComponent(flagBearerEntity, new FlagBearer {
                        formationWidth = 3,
                        formationHeight = 3,
                        unitSpacing = 2f,
                        formationDistance = 3f,
                        moveSpeed = spawner.ValueRO.moveSpeed,
                        rotationSpeed = 10f,
                        targetPosition = hasValidTarget ? targetPos : formationSpawnPos,
                        isMoving = hasValidTarget
                    });
                    
                    // 2. Erstelle FormationFollower Units (8 Units in 3x3 Formation, ohne die Mitte)
                    int followerCount = 8; // 3x3 - 1 (FlagBearer in der Mitte)
                    int formationWidth = 3;
                    
                    for (int i = 0; i < followerCount; i++) {
                        Entity followerEntity = entityCommandBuffer.Instantiate(spawner.ValueRO.prefabEntity);
                        
                        // Berechne Formation Position (überspringe die Mitte für FlagBearer)
                        int adjustedIndex = i >= 4 ? i + 1 : i; // Überspringe Index 4 (Mitte)
                        int column = adjustedIndex % formationWidth;
                        int row = adjustedIndex / formationWidth;
                        int2 formationPos = new int2(column, row);
                        
                        // Setze initiale Position leicht versetzt
                        float3 followerSpawnPos = formationSpawnPos + new float3(
                            (column - 1) * 2f, // -1 um zu zentrieren
                            0,
                            (row - 1) * 2f
                        );
                        
                        entityCommandBuffer.SetComponent(followerEntity, LocalTransform.FromPosition(followerSpawnPos));
                        
                        // Entferne UnitMover und SetupUnitMoverDefaultPosition Komponenten
                        entityCommandBuffer.RemoveComponent<UnitMover>(followerEntity);
                        entityCommandBuffer.RemoveComponent<SetupUnitMoverDefaultPosition>(followerEntity);
                        
                        // Füge FormationFollower Komponente hinzu
                        entityCommandBuffer.AddComponent(followerEntity, new FormationFollower {
                            flagBearerEntity = flagBearerEntity,
                            formationPosition = formationPos,
                            targetPosition = float3.zero,
                            moveSpeed = spawner.ValueRO.moveSpeed,
                            rotationSpeed = 10f,
                            isMoving = false,
                            shouldResetToFormation = false
                        });
                    }
                    
                    #if UNITY_EDITOR
                    Debug.Log($"Created Formation: 1 FlagBearer + {followerCount} Followers - Target: {(hasValidTarget ? targetPos : formationSpawnPos)}");
                    #endif
                }

                // Timer zurücksetzen
                spawner.ValueRW.timer = spawner.ValueRO.spawnInterval;
            }
        }
    }
}
