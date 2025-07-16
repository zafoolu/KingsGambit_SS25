using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

public class UnitProductionCursedOnesUI : MonoBehaviour {
    [Header("Cursed Unit Buttons")]
    [SerializeField] private Button cursedPawnButton;
    [SerializeField] private Button cursedQueenButton;
    [SerializeField] private Button cursedRookButton;
    [SerializeField] private Button cursedBishopButton;
    [SerializeField] private Button cursedKnightButton;
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;

    private EntityManager entityManager;

    private void Awake() {
        Debug.Log("UnitProductionCursedOnesUI Awake() called!");
        
        // Button Event Listeners
        if (cursedPawnButton != null)
            cursedPawnButton.onClick.AddListener(() => SpawnUnit(UnitTypeSO.UnitType.CursedPawn));
        
        if (cursedQueenButton != null)
            cursedQueenButton.onClick.AddListener(() => SpawnUnit(UnitTypeSO.UnitType.CursedQueen));
        
        if (cursedRookButton != null)
            cursedRookButton.onClick.AddListener(() => SpawnUnit(UnitTypeSO.UnitType.CursedRook));
        
        if (cursedBishopButton != null)
            cursedBishopButton.onClick.AddListener(() => SpawnUnit(UnitTypeSO.UnitType.CursedBishop));
        
        if (cursedKnightButton != null)
            cursedKnightButton.onClick.AddListener(() => SpawnUnit(UnitTypeSO.UnitType.CursedKnight));
    }

    private void Start() {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        // Set default spawn point if not assigned
        if (spawnPoint == null) {
            spawnPoint = transform;
        }
    }

    private bool IsEntityManagerReady() {
        try {
            if (entityManager == null) {
                Debug.LogWarning("EntityManager is null");
                return false;
            }
            
            if (!entityManager.World.IsCreated) {
                Debug.LogWarning("EntityManager world is not created");
                return false;
            }
            
            entityManager.CreateEntityQuery(typeof(LocalTransform));
            return true;
        } catch (System.Exception e) {
            Debug.LogError("EntityManager is not ready: " + e.Message);
            return false;
        }
    }

    private bool HasRequiredBuildingForUnit(UnitTypeSO.UnitType unitType) {
        try {
            if (!IsEntityManagerReady()) {
                Debug.LogWarning("EntityManager not ready, cannot check for buildings");
                return false;
            }

            EntityQuery buildingQuery;
            string buildingName;

            switch (unitType) {
                case UnitTypeSO.UnitType.CursedPawn:
                    buildingQuery = entityManager.CreateEntityQuery(typeof(BuildingRuins));
                    buildingName = "Ruinen";
                    break;
                case UnitTypeSO.UnitType.CursedQueen:
                    buildingQuery = entityManager.CreateEntityQuery(typeof(BuildingBloodcircle));
                    buildingName = "Blutkreis";
                    break;
                case UnitTypeSO.UnitType.CursedRook:
                case UnitTypeSO.UnitType.CursedBishop:
                case UnitTypeSO.UnitType.CursedKnight:
                    buildingQuery = entityManager.CreateEntityQuery(typeof(BuildingCathedral));
                    buildingName = "Kathedrale";
                    break;
                default:
                    Debug.LogWarning($"Unbekannter Einheitentyp: {unitType}");
                    return false;
            }

            int buildingCount = buildingQuery.CalculateEntityCount();
            Debug.Log($"Found {buildingCount} {buildingName} in scene for {unitType}");
            
            if (buildingCount == 0) {
                Debug.LogWarning($"Kein {buildingName} in der Szene gefunden! {unitType} kann nur bei {buildingName} gespawnt werden.");
                return false;
            }
            
            return true;
        } catch (System.Exception e) {
            Debug.LogError($"Error checking for required building for {unitType}: " + e.Message);
            return false;
        }
    }

    public void SpawnUnit(UnitTypeSO.UnitType unitType) {
        Debug.Log("SpawnUnit called for: " + unitType);
        
        // Null checks for critical components
        if (GameAssets.Instance == null) {
            Debug.LogError("GameAssets.Instance is null!");
            return;
        }
        
        if (GameAssets.Instance.unitTypeListSO == null) {
            Debug.LogError("GameAssets.Instance.unitTypeListSO is null!");
            return;
        }
        
        UnitTypeSO unitTypeSO = GameAssets.Instance.unitTypeListSO.GetUnitTypeSO(unitType);
        if (unitTypeSO == null) {
            Debug.LogError("UnitTypeSO is null for unit type: " + unitType);
            return;
        }
        
        if (ResourceManager.Instance == null) {
            Debug.LogError("ResourceManager.Instance is null!");
            return;
        }
        
        if (unitTypeSO.spawnCostResourceAmountArray == null) {
            Debug.LogError("spawnCostResourceAmountArray is null for unit type: " + unitType);
            return;
        }
        
        // Check if EntityManager is ready
        if (!IsEntityManagerReady()) {
            Debug.LogError("EntityManager not ready, cannot spawn unit");
            return;
        }
        
        // Check if required building exists in scene
        if (!HasRequiredBuildingForUnit(unitType)) {
            return; // Error message already logged in HasRequiredBuildingForUnit
        }
        
        if (!ResourceManager.Instance.CanSpendResourceAmount(unitTypeSO.spawnCostResourceAmountArray)) {
            Debug.Log("Nicht genug Ressourcen für " + unitType);
            return;
        }
        
        float3 spawnPosition = FindSpawnPositionForUnit(unitType);
        
        ResourceManager.Instance.SpendResourceAmount(unitTypeSO.spawnCostResourceAmountArray);
        
        try {
            EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(EntitiesReferences));
            EntitiesReferences entitiesReferences = entityQuery.GetSingleton<EntitiesReferences>();
            
            SpawnFormation(unitTypeSO, entitiesReferences, spawnPosition);
            
            Debug.Log("Successfully spawned " + unitType);
        } catch (System.Exception e) {
            Debug.LogError("Error spawning unit " + unitType + ": " + e.Message);
            // Refund resources if spawning failed
            try {
                if (unitTypeSO != null && unitTypeSO.spawnCostResourceAmountArray != null && ResourceManager.Instance != null) {
                    foreach (ResourceAmount resourceAmount in unitTypeSO.spawnCostResourceAmountArray) {
                        ResourceManager.Instance.AddResourceAmount(resourceAmount.resourceType, resourceAmount.amount);
                    }
                    Debug.Log("Resources refunded for failed spawn of " + unitType);
                }
            } catch (System.Exception refundException) {
                Debug.LogError("Error refunding resources: " + refundException.Message);
            }
        }
    }

    private float3 FindSpawnPositionForUnit(UnitTypeSO.UnitType unitType) {
        switch (unitType) {
            case UnitTypeSO.UnitType.CursedPawn:
                return FindRuinsSpawnPosition();
            case UnitTypeSO.UnitType.CursedQueen:
                return FindBloodcircleSpawnPosition();
            case UnitTypeSO.UnitType.CursedRook:
            case UnitTypeSO.UnitType.CursedBishop:
            case UnitTypeSO.UnitType.CursedKnight:
                return FindCathedralSpawnPosition();
            default:
                Debug.LogWarning($"Unknown unit type for spawn position: {unitType}");
                return GetSafeFallbackPosition();
        }
    }

    private float3 FindRuinsSpawnPosition() {
        if (entityManager == null) {
            if (World.DefaultGameObjectInjectionWorld != null) {
                entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                Debug.Log("EntityManager initialized in FindRuinsSpawnPosition");
            } else {
                Debug.LogError("World.DefaultGameObjectInjectionWorld is null in FindRuinsSpawnPosition!");
                return GetSafeFallbackPosition();
            }
        }
        
        try {
            EntityQuery ruinsQuery = entityManager.CreateEntityQuery(typeof(BuildingRuins), typeof(LocalTransform));
        
            if (ruinsQuery.CalculateEntityCount() > 0) {
                NativeArray<LocalTransform> ruinsTransforms = ruinsQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
                
                if (ruinsTransforms.Length > 0) {
                    float3 ruinsPosition = ruinsTransforms[0].Position;
                    
                    if (!IsValidPosition(ruinsPosition)) {
                        Debug.LogWarning("Ruins has invalid position, using fallback");
                        ruinsTransforms.Dispose();
                        return GetSafeFallbackPosition();
                    }
                    
                    float3 spawnOffset = new float3(3f, 0f, 3f);
                    float3 spawnPosition = ruinsPosition + spawnOffset;
                    
                    if (!IsValidPosition(spawnPosition)) {
                        Debug.LogWarning("Calculated spawn position is invalid, using fallback");
                        ruinsTransforms.Dispose();
                        return GetSafeFallbackPosition();
                    }
                    
                    ruinsTransforms.Dispose();
                    
                    Debug.Log($"Spawning near ruins at position: {spawnPosition}");
                    return spawnPosition;
                }
                
                ruinsTransforms.Dispose();
            }
            
            float3 fallbackPosition = GetSafeFallbackPosition();
            Debug.Log($"No valid ruins found, using fallback position: {fallbackPosition}");
            return fallbackPosition;
        } catch (System.Exception e) {
            Debug.LogError("Error in FindRuinsSpawnPosition: " + e.Message);
            return GetSafeFallbackPosition();
        }
    }

    private float3 FindBloodcircleSpawnPosition() {
        if (entityManager == null) {
            if (World.DefaultGameObjectInjectionWorld != null) {
                entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                Debug.Log("EntityManager initialized in FindBloodcircleSpawnPosition");
            } else {
                Debug.LogError("World.DefaultGameObjectInjectionWorld is null in FindBloodcircleSpawnPosition!");
                return GetSafeFallbackPosition();
            }
        }
        
        try {
            EntityQuery bloodcircleQuery = entityManager.CreateEntityQuery(typeof(BuildingBloodcircle), typeof(LocalTransform));
        
            if (bloodcircleQuery.CalculateEntityCount() > 0) {
                NativeArray<LocalTransform> bloodcircleTransforms = bloodcircleQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
                
                if (bloodcircleTransforms.Length > 0) {
                    float3 bloodcirclePosition = bloodcircleTransforms[0].Position;
                    
                    if (!IsValidPosition(bloodcirclePosition)) {
                        Debug.LogWarning("Bloodcircle has invalid position, using fallback");
                        bloodcircleTransforms.Dispose();
                        return GetSafeFallbackPosition();
                    }
                    
                    float3 spawnOffset = new float3(3f, 0f, 3f);
                    float3 spawnPosition = bloodcirclePosition + spawnOffset;
                    
                    if (!IsValidPosition(spawnPosition)) {
                        Debug.LogWarning("Calculated spawn position is invalid, using fallback");
                        bloodcircleTransforms.Dispose();
                        return GetSafeFallbackPosition();
                    }
                    
                    bloodcircleTransforms.Dispose();
                    
                    Debug.Log($"Spawning near bloodcircle at position: {spawnPosition}");
                    return spawnPosition;
                }
                
                bloodcircleTransforms.Dispose();
            }
            
            float3 fallbackPosition = GetSafeFallbackPosition();
            Debug.Log($"No valid bloodcircle found, using fallback position: {fallbackPosition}");
            return fallbackPosition;
        } catch (System.Exception e) {
            Debug.LogError("Error in FindBloodcircleSpawnPosition: " + e.Message);
            return GetSafeFallbackPosition();
        }
    }

    private float3 FindCathedralSpawnPosition() {
        if (entityManager == null) {
            if (World.DefaultGameObjectInjectionWorld != null) {
                entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                Debug.Log("EntityManager initialized in FindCathedralSpawnPosition");
            } else {
                Debug.LogError("World.DefaultGameObjectInjectionWorld is null in FindCathedralSpawnPosition!");
                return GetSafeFallbackPosition();
            }
        }
        
        try {
            EntityQuery cathedralQuery = entityManager.CreateEntityQuery(typeof(BuildingCathedral), typeof(LocalTransform));
        
            if (cathedralQuery.CalculateEntityCount() > 0) {
                NativeArray<LocalTransform> cathedralTransforms = cathedralQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
                
                if (cathedralTransforms.Length > 0) {
                    float3 cathedralPosition = cathedralTransforms[0].Position;
                    
                    if (!IsValidPosition(cathedralPosition)) {
                        Debug.LogWarning("Cathedral has invalid position, using fallback");
                        cathedralTransforms.Dispose();
                        return GetSafeFallbackPosition();
                    }
                    
                    float3 spawnOffset = new float3(3f, 0f, 3f);
                    float3 spawnPosition = cathedralPosition + spawnOffset;
                    
                    if (!IsValidPosition(spawnPosition)) {
                        Debug.LogWarning("Calculated spawn position is invalid, using fallback");
                        cathedralTransforms.Dispose();
                        return GetSafeFallbackPosition();
                    }
                    
                    cathedralTransforms.Dispose();
                    
                    Debug.Log($"Spawning near cathedral at position: {spawnPosition}");
                    return spawnPosition;
                }
                
                cathedralTransforms.Dispose();
            }
            
            float3 fallbackPosition = GetSafeFallbackPosition();
            Debug.Log($"No valid cathedral found, using fallback position: {fallbackPosition}");
            return fallbackPosition;
        } catch (System.Exception e) {
            Debug.LogError("Error in FindCathedralSpawnPosition: " + e.Message);
            return GetSafeFallbackPosition();
        }
    }

    private float3 GetSafeFallbackPosition() {
        if (spawnPoint != null) {
            try {
                Vector3 pos = spawnPoint.position;
                float3 result = new float3(pos.x, pos.y, pos.z);
                
                if (IsValidPosition(result)) {
                    return result;
                } else {
                    Debug.LogWarning("spawnPoint position contains invalid values (NaN/Infinity), using fallback");
                }
            } catch (System.Exception e) {
                Debug.LogError("Error accessing spawnPoint position: " + e.Message);
            }
        }
        
        if (transform != null) {
            try {
                Vector3 pos = transform.position;
                float3 result = new float3(pos.x, pos.y, pos.z);
                
                if (IsValidPosition(result)) {
                    return result;
                } else {
                    Debug.LogWarning("transform position contains invalid values (NaN/Infinity), using world origin");
                }
            } catch (System.Exception e) {
                Debug.LogError("Error accessing transform position: " + e.Message);
            }
        }
        
        Debug.LogWarning("Using world origin (0,0,0) as spawn position");
        return new float3(0, 0, 0);
    }

    private bool IsValidPosition(float3 position) {
        return math.isfinite(position.x) && math.isfinite(position.y) && math.isfinite(position.z);
    }

    private void SpawnFormation(UnitTypeSO unitTypeSO, EntitiesReferences entitiesReferences, float3 spawnPosition) {
        if (!IsValidPosition(spawnPosition)) {
            Debug.LogError("Invalid spawn position detected in SpawnFormation: " + spawnPosition + ", using fallback");
            spawnPosition = GetSafeFallbackPosition();
        }
        
        int formationAmount = unitTypeSO.formationAmount;
        
        if (formationAmount <= 1) {
            // Spawn single unit if formation amount is 1 or less
            Entity spawnedUnitEntity = entityManager.Instantiate(unitTypeSO.GetPrefabEntity(entitiesReferences));
            entityManager.SetComponentData(spawnedUnitEntity, LocalTransform.FromPosition(spawnPosition));
        } else {
            // Spawn formation with flag bearer and followers
            int formationWidth = Mathf.CeilToInt(Mathf.Sqrt(formationAmount));
            int formationHeight = Mathf.CeilToInt((float)formationAmount / formationWidth);
            
            // Spawn Flag Bearer
            Entity flagBearerEntity = entityManager.Instantiate(unitTypeSO.GetFlagbearerPrefabEntity(entitiesReferences));
            entityManager.SetComponentData(flagBearerEntity, LocalTransform.FromPosition(spawnPosition));
            
            // Configure Flag Bearer
            entityManager.SetComponentData(flagBearerEntity, new FlagBearer {
                formationWidth = formationWidth,
                formationHeight = formationHeight,
                unitSpacing = 2f,
                formationDistance = 1f,
                moveSpeed = 5f,
                rotationSpeed = 5f,
                targetPosition = spawnPosition,
                isMoving = false
            });
            
            // Spawn Formation Followers
            for (int i = 1; i < formationAmount; i++) {
                Entity followerEntity = entityManager.Instantiate(unitTypeSO.GetPrefabEntity(entitiesReferences));
                entityManager.SetComponentData(followerEntity, LocalTransform.FromPosition(spawnPosition));
                
                // Make this unit a formation follower
                entityManager.AddComponent<FormationFollower>(followerEntity);
                
                // Calculate formation position (column, row)
                int column = (i - 1) % formationWidth;
                int row = (i - 1) / formationWidth;
                int2 formationPos = new int2(column, row);
                
                entityManager.SetComponentData(followerEntity, new FormationFollower {
                    flagBearerEntity = flagBearerEntity,
                    formationPosition = formationPos,
                    targetPosition = spawnPosition,
                    moveSpeed = 5f,
                    rotationSpeed = 5f,
                    isMoving = false,
                    shouldResetToFormation = false
                });
            }
        }
    }
}