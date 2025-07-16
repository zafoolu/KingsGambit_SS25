using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct BuildingCathedralSystem : ISystem {

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<EntitiesReferences>();
    }

    public void OnUpdate(ref SystemState state) {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        // Handle unit enqueue requests
        foreach ((
            RefRW<BuildingCathedral> buildingCathedral,
            DynamicBuffer<SpawnUnitTypeBuffer> spawnUnitTypeDynamicBuffer,
            RefRO<BuildingBarracksUnitEnqueue> buildingBarracksUnitEnqueue,
            EnabledRefRW<BuildingBarracksUnitEnqueue> buildingBarracksUnitEnqueueEnabled)
            in SystemAPI.Query<
                RefRW<BuildingCathedral>,
                DynamicBuffer<SpawnUnitTypeBuffer>,
                RefRO<BuildingBarracksUnitEnqueue>,
                EnabledRefRW<BuildingBarracksUnitEnqueue>>()) {

            spawnUnitTypeDynamicBuffer.Add(new SpawnUnitTypeBuffer {
                unitType = buildingBarracksUnitEnqueue.ValueRO.unitType
            });
            buildingBarracksUnitEnqueueEnabled.ValueRW = false;

            buildingCathedral.ValueRW.onUnitQueueChanged = true;
        }

        // Handle unit spawning
        foreach ((
            RefRO<LocalTransform> localTransform,
            RefRW<BuildingCathedral> buildingCathedral,
            DynamicBuffer<SpawnUnitTypeBuffer> spawnUnitTypeDynamicBuffer)
            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRW<BuildingCathedral>,
                DynamicBuffer<SpawnUnitTypeBuffer>>()) {

            if (spawnUnitTypeDynamicBuffer.IsEmpty) {
                continue;
            }

            if (buildingCathedral.ValueRO.activeUnitType != spawnUnitTypeDynamicBuffer[0].unitType) {
                buildingCathedral.ValueRW.activeUnitType = spawnUnitTypeDynamicBuffer[0].unitType;

                UnitTypeSO activeUnitTypeSO = 
                    GameAssets.Instance.unitTypeListSO.GetUnitTypeSO(buildingCathedral.ValueRO.activeUnitType);

                buildingCathedral.ValueRW.progressMax = activeUnitTypeSO.progressMax;
            }

            buildingCathedral.ValueRW.progress += SystemAPI.Time.DeltaTime;

            if (buildingCathedral.ValueRO.progress < buildingCathedral.ValueRO.progressMax) {
                continue;
            }

            buildingCathedral.ValueRW.progress = 0f;

            UnitTypeSO.UnitType unitType = spawnUnitTypeDynamicBuffer[0].unitType;
            UnitTypeSO unitTypeSO = GameAssets.Instance.unitTypeListSO.GetUnitTypeSO(unitType);

            spawnUnitTypeDynamicBuffer.RemoveAt(0);
            buildingCathedral.ValueRW.onUnitQueueChanged = true;

            // Spawn formation instead of single unit
            this.SpawnFormation(ref state, unitTypeSO, entitiesReferences, localTransform.ValueRO.Position, buildingCathedral.ValueRO.rallyPositionOffset);
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