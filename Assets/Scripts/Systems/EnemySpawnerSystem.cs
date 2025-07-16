using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// ECS-System für automatisches Enemy-Spawning mit Formation-Support
/// </summary>
[BurstCompile]
public partial struct EnemySpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (spawner, transform) in 
            SystemAPI.Query<RefRW<EnemySpawner>, RefRO<LocalTransform>>())
        {
            if (!spawner.ValueRO.isActive) continue;

            // Timer aktualisieren
            spawner.ValueRW.currentSpawnTimer -= deltaTime;

            // Prüfen ob gespawnt werden soll
            if (spawner.ValueRO.currentSpawnTimer <= 0f)
            {
                // Prüfen ob maximale Spawns erreicht
                if (spawner.ValueRO.maxSpawns > 0 && 
                    spawner.ValueRO.currentSpawnCount >= spawner.ValueRO.maxSpawns)
                {
                    spawner.ValueRW.isActive = false;
                    continue;
                }

                // UnitTypeSO holen - genau wie im BuildingBarracksSystem
                UnitTypeSO unitTypeSO = GameAssets.Instance.unitTypeListSO.GetUnitTypeSO(spawner.ValueRO.unitType);

                // Formation spawnen - genau wie im BuildingBarracksSystem
                this.SpawnFormation(ref state, unitTypeSO, entitiesReferences, transform.ValueRO.Position, spawner.ValueRO.rallyPositionOffset);

                // Timer und Counter aktualisieren
                spawner.ValueRW.currentSpawnTimer = spawner.ValueRO.spawnInterval;
                spawner.ValueRW.currentSpawnCount++;
            }
        }
    }

    private void SpawnFormation(ref SystemState state, UnitTypeSO unitTypeSO, EntitiesReferences entitiesReferences, float3 spawnPosition, float3 rallyPositionOffset)
    {
        int formationAmount = unitTypeSO.formationAmount;
        
        if (formationAmount <= 1)
        {
            // Spawn single unit if formation amount is 1 or less
            Entity spawnedUnitEntity = state.EntityManager.Instantiate(unitTypeSO.GetPrefabEntity(entitiesReferences));
            state.EntityManager.SetComponentData(spawnedUnitEntity, LocalTransform.FromPosition(spawnPosition));
            
            // Set MoveOverride for single units
            if (!state.EntityManager.HasComponent<FormationFollower>(spawnedUnitEntity) && 
                !state.EntityManager.HasComponent<FlagBearer>(spawnedUnitEntity))
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
        
        // Set MoveOverride for Flag Bearer to move to rally position
        state.EntityManager.SetComponentData(flagBearerEntity, new MoveOverride {
            targetPosition = spawnPosition + rallyPositionOffset
        });
        state.EntityManager.SetComponentEnabled<MoveOverride>(flagBearerEntity, true);
        
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