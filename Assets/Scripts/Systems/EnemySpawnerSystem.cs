using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// ECS System für automatisches Enemy-Spawning
/// Übernimmt die komplette Logik vom BuildingBarracksSystem aber spawnt automatisch
/// </summary>
partial struct EnemySpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        foreach ((
            RefRO<LocalTransform> localTransform,
            RefRW<EnemySpawner> enemySpawner)
            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRW<EnemySpawner>>())
        {
            // Prüfe ob Spawner aktiv ist
            if (!enemySpawner.ValueRO.isActive)
            {
                continue;
            }

            // Prüfe Max Spawns (0 = unendlich)
            if (enemySpawner.ValueRO.maxSpawns > 0 && 
                enemySpawner.ValueRO.currentSpawnCount >= enemySpawner.ValueRO.maxSpawns)
            {
                enemySpawner.ValueRW.isActive = false;
                continue;
            }

            // Hole UnitTypeSO für progressMax (wie im BuildingBarracksSystem)
            UnitTypeSO unitTypeSO = GameAssets.Instance.unitTypeListSO.GetUnitTypeSO(enemySpawner.ValueRO.unitType);
            
            // Update Progress Timer
            enemySpawner.ValueRW.progress += SystemAPI.Time.DeltaTime;

            // Prüfe ob Spawn-Zeit erreicht ist (verwende progressMax vom UnitTypeSO)
            if (enemySpawner.ValueRO.progress < unitTypeSO.progressMax)
            {
                continue;
            }

            // Reset Progress Timer
            enemySpawner.ValueRW.progress = 0f;

            // Erhöhe Spawn Counter
            enemySpawner.ValueRW.currentSpawnCount++;

            // Spawn Formation (identisch zum BuildingBarracksSystem)
            SpawnFormation(ref state, unitTypeSO, entitiesReferences, 
                          localTransform.ValueRO.Position, enemySpawner.ValueRO.rallyPositionOffset);
        }
    }

    /// <summary>
    /// Identische SpawnFormation Methode vom BuildingBarracksSystem
    /// </summary>
    private void SpawnFormation(ref SystemState state, UnitTypeSO unitTypeSO, EntitiesReferences entitiesReferences, 
                               float3 spawnPosition, float3 rallyPositionOffset)
    {
        int formationAmount = unitTypeSO.formationAmount;
        
        if (formationAmount <= 1)
        {
            // Spawn single unit if formation amount is 1 or less
            Entity spawnedUnitEntity = state.EntityManager.Instantiate(unitTypeSO.GetPrefabEntity(entitiesReferences));
            state.EntityManager.SetComponentData(spawnedUnitEntity, LocalTransform.FromPosition(spawnPosition));
            
            // Set MoveOverride for single units (only if component exists)
            if (!state.EntityManager.HasComponent<FormationFollower>(spawnedUnitEntity) && 
                !state.EntityManager.HasComponent<FlagBearer>(spawnedUnitEntity) &&
                state.EntityManager.HasComponent<MoveOverride>(spawnedUnitEntity))
            {
                state.EntityManager.SetComponentData(spawnedUnitEntity, new MoveOverride {
                    targetPosition = spawnPosition + rallyPositionOffset
                });
                state.EntityManager.SetComponentEnabled<MoveOverride>(spawnedUnitEntity, true);
            }
            return;
        }

        // Calculate formation parameters
        int followerCount = formationAmount - 1; // One unit becomes the flag bearer
        int formationWidth = FormationUtility.CalculateOptimalFormationWidth(followerCount);
        int formationHeight = FormationUtility.CalculateFormationHeight(followerCount, formationWidth);
        
        // Spawn Flag Bearer
        Entity flagBearerEntity = state.EntityManager.Instantiate(unitTypeSO.GetFlagbearerPrefabEntity(entitiesReferences));
        state.EntityManager.SetComponentData(flagBearerEntity, LocalTransform.FromPosition(spawnPosition));
        
        // Configure Flag Bearer
        state.EntityManager.SetComponentData(flagBearerEntity, new FlagBearer
        {
            formationWidth = formationWidth,
            formationHeight = formationHeight,
            unitSpacing = 2f,
            formationDistance = 1f,
            moveSpeed = 5f,
            rotationSpeed = 5f,
            targetPosition = spawnPosition + rallyPositionOffset,
            isMoving = false
        });
        
        // Set MoveOverride for Flag Bearer to move to rally position (only if component exists)
        if (state.EntityManager.HasComponent<MoveOverride>(flagBearerEntity))
        {
            state.EntityManager.SetComponentData(flagBearerEntity, new MoveOverride {
                targetPosition = spawnPosition + rallyPositionOffset
            });
            state.EntityManager.SetComponentEnabled<MoveOverride>(flagBearerEntity, true);
        }
        
        // Spawn Formation Followers
        for (int i = 0; i < followerCount; i++)
        {
            Entity followerEntity = state.EntityManager.Instantiate(unitTypeSO.GetPrefabEntity(entitiesReferences));
            state.EntityManager.SetComponentData(followerEntity, LocalTransform.FromPosition(spawnPosition));
            
            // Calculate formation position
            int2 formationPos = FormationUtility.IndexToFormationPosition(i, formationWidth);
            
            // Configure Formation Follower
            state.EntityManager.SetComponentData(followerEntity, new FormationFollower
            {
                flagBearerEntity = flagBearerEntity,
                formationPosition = formationPos,
                targetPosition = float3.zero,
                moveSpeed = 5f,
                rotationSpeed = 5f,
                isMoving = false,
                shouldResetToFormation = false
            });
        }
    }

}