using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

public class BuildingBarracksUI : MonoBehaviour {
    [SerializeField] private Button carraraQueenButton;
    [SerializeField] private Button carraraBishopButton;
    [SerializeField] private Button carraraKnightButton;
    [SerializeField] private Button carraraRookButton;
    [SerializeField] private Button carraraPawnButton;
    [SerializeField] private Transform spawnPoint;

    private EntityManager entityManager;

    private void Awake() {
        Debug.Log("BuildingBarracksUI Awake() called!");
        
        carraraQueenButton.onClick.AddListener(() => {
            SpawnUnit(UnitTypeSO.UnitType.CarraraQueen);
        });

        carraraBishopButton.onClick.AddListener(() => {
            SpawnUnit(UnitTypeSO.UnitType.CarraraBishop);
        });

        carraraKnightButton.onClick.AddListener(() => {
            SpawnUnit(UnitTypeSO.UnitType.CarraraKnight);
        });

        carraraRookButton.onClick.AddListener(() => {
            Debug.Log("ROOK BUTTON CLICKED!");
            SpawnUnit(UnitTypeSO.UnitType.CarraraRook);
        });

        carraraPawnButton.onClick.AddListener(() => {
            SpawnUnit(UnitTypeSO.UnitType.CarraraPawn);
        });
    }

    // Test method - call this manually from Inspector or another script
    [ContextMenu("Test Rook Spawn")]
    public void TestRookSpawn() {
        Debug.Log("TEST ROOK SPAWN CALLED!");
        SpawnUnit(UnitTypeSO.UnitType.CarraraRook);
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
            // Check if EntityManager is null
            if (entityManager == null) {
                Debug.LogWarning("EntityManager is null");
                return false;
            }
            
            // Check if the world is created
            if (!entityManager.World.IsCreated) {
                Debug.LogWarning("EntityManager world is not created");
                return false;
            }
            
            // Try a simple operation to verify functionality
            entityManager.CreateEntityQuery(typeof(LocalTransform));
            return true;
        } catch (System.Exception e) {
            Debug.LogError("EntityManager is not ready: " + e.Message);
            return false;
        }
    }

    private bool HasBarracksInScene() {
        try {
            if (!IsEntityManagerReady()) {
                Debug.LogWarning("EntityManager not ready, cannot check for barracks");
                return false;
            }
             
            EntityQuery barracksQuery = entityManager.CreateEntityQuery(typeof(BuildingBarracks));
            int barracksCount = barracksQuery.CalculateEntityCount();
            
            Debug.Log($"Found {barracksCount} barracks in scene");
            return barracksCount > 0;
        } catch (System.Exception e) {
            Debug.LogError("Error checking for barracks: " + e.Message);
            return false;
        }
    }

    private bool HasWorkshopsInScene() {
        try {
            if (!IsEntityManagerReady()) {
                Debug.LogWarning("EntityManager not ready, cannot check for workshops");
                return false;
            }
             
            EntityQuery workshopQuery = entityManager.CreateEntityQuery(typeof(BuildingWorkshop));
            int workshopCount = workshopQuery.CalculateEntityCount();
            
            Debug.Log($"Found {workshopCount} workshops in scene");
            return workshopCount > 0;
        } catch (System.Exception e) {
            Debug.LogError("Error checking for workshops: " + e.Message);
            return false;
        }
    }

    private bool IsPawnUnit(UnitTypeSO.UnitType unitType) {
        return unitType == UnitTypeSO.UnitType.CarraraPawn;
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
        
        // Check if the appropriate building type exists before allowing spawn
        bool isPawn = IsPawnUnit(unitType);
        if (isPawn) {
            // Pawns can only be spawned at barracks
            if (!HasBarracksInScene()) {
                Debug.LogWarning("Keine Kasernen in der Szene gefunden! Bauern können nur bei Kasernen gespawnt werden.");
                return;
            }
        } else {
            // All other units can only be spawned at workshops
            if (!HasWorkshopsInScene()) {
                Debug.LogWarning("Keine Werkstätten in der Szene gefunden! Diese Einheit kann nur bei Werkstätten gespawnt werden.");
                return;
            }
        }
        
        if (!ResourceManager.Instance.CanSpendResourceAmount(unitTypeSO.spawnCostResourceAmountArray)) {
            Debug.Log("Nicht genug Ressourcen für " + unitType);
            return;
        }
        
        float3 spawnPosition;
        if (isPawn) {
            spawnPosition = FindBarracksSpawnPosition();
        } else {
            spawnPosition = FindWorkshopSpawnPosition();
        }
        
        ResourceManager.Instance.SpendResourceAmount(unitTypeSO.spawnCostResourceAmountArray);
        
        try {
            EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(EntitiesReferences));
            EntitiesReferences entitiesReferences = entityQuery.GetSingleton<EntitiesReferences>();
            
            SpawnFormation(unitTypeSO, entitiesReferences, spawnPosition);
            
            Debug.Log("Successfully spawned " + unitType);
        } catch (System.Exception e) {
            Debug.LogError("Error spawning unit " + unitType + ": " + e.Message);
            Debug.LogError("Stack trace: " + e.StackTrace);
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

    private float3 FindWorkshopSpawnPosition() {
        // Try to initialize EntityManager if it's null
        if (entityManager == null) {
            if (World.DefaultGameObjectInjectionWorld != null) {
                entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                Debug.Log("EntityManager initialized in FindWorkshopSpawnPosition");
            } else {
                Debug.LogError("World.DefaultGameObjectInjectionWorld is null in FindWorkshopSpawnPosition!");
                // Use safe fallback position
                return GetSafeFallbackPosition();
            }
        }
        
        // Additional safety check - verify EntityManager is valid before using it
        try {
            // Query for all workshop entities
            EntityQuery workshopQuery = entityManager.CreateEntityQuery(typeof(BuildingWorkshop), typeof(LocalTransform));
        
            if (workshopQuery.CalculateEntityCount() > 0) {
                // Get the first workshop found
                NativeArray<Entity> workshopEntities = workshopQuery.ToEntityArray(Allocator.Temp);
                NativeArray<LocalTransform> workshopTransforms = workshopQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
                
                if (workshopEntities.Length > 0 && workshopTransforms.Length > 0) {
                    // Use the position of the first workshop, with a small offset for spawning
                    float3 workshopPosition = workshopTransforms[0].Position;
                    
                    // Validate workshop position
                    if (!IsValidPosition(workshopPosition)) {
                        Debug.LogWarning("Workshop has invalid position, using fallback");
                        workshopEntities.Dispose();
                        workshopTransforms.Dispose();
                        return GetSafeFallbackPosition();
                    }
                    
                    float3 spawnOffset = new float3(3f, 0f, 3f); // Spawn 3 units away from workshop
                    float3 spawnPosition = workshopPosition + spawnOffset;
                    
                    // Validate final spawn position
                    if (!IsValidPosition(spawnPosition)) {
                        Debug.LogWarning("Calculated spawn position is invalid, using fallback");
                        workshopEntities.Dispose();
                        workshopTransforms.Dispose();
                        return GetSafeFallbackPosition();
                    }
                    
                    workshopEntities.Dispose();
                    workshopTransforms.Dispose();
                    
                    Debug.Log($"Spawning near workshop at position: {spawnPosition}");
                    return spawnPosition;
                }
                
                workshopEntities.Dispose();
                workshopTransforms.Dispose();
            }
            
            // If no workshop found, use the default spawn point or world origin
            float3 fallbackPosition = GetSafeFallbackPosition();
            Debug.Log($"No valid workshop found, using fallback position: {fallbackPosition}");
            return fallbackPosition;
        } catch (System.Exception e) {
            Debug.LogError("Error in FindWorkshopSpawnPosition: " + e.Message);
            Debug.LogError("Using fallback spawn position");
            // Use safe fallback position
            return GetSafeFallbackPosition();
        }
    }

    private float3 FindBarracksSpawnPosition() {
        // Try to initialize EntityManager if it's null
        if (entityManager == null) {
            if (World.DefaultGameObjectInjectionWorld != null) {
                entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                Debug.Log("EntityManager initialized in FindBarracksSpawnPosition");
            } else {
                Debug.LogError("World.DefaultGameObjectInjectionWorld is null in FindBarracksSpawnPosition!");
                // Use safe fallback position
                return GetSafeFallbackPosition();
            }
        }
        
        // Additional safety check - verify EntityManager is valid before using it
        try {
            // Query for all barracks entities
            EntityQuery barracksQuery = entityManager.CreateEntityQuery(typeof(BuildingBarracks), typeof(LocalTransform));
        
            if (barracksQuery.CalculateEntityCount() > 0) {
                // Get the first barracks found
                NativeArray<Entity> barracksEntities = barracksQuery.ToEntityArray(Allocator.Temp);
                NativeArray<LocalTransform> barracksTransforms = barracksQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
                
                if (barracksEntities.Length > 0 && barracksTransforms.Length > 0) {
                    // Use the position of the first barracks, with a small offset for spawning
                    float3 barracksPosition = barracksTransforms[0].Position;
                    
                    // Validate barracks position
                    if (!IsValidPosition(barracksPosition)) {
                        Debug.LogWarning("Barracks has invalid position, using fallback");
                        barracksEntities.Dispose();
                        barracksTransforms.Dispose();
                        return GetSafeFallbackPosition();
                    }
                    
                    float3 spawnOffset = new float3(3f, 0f, 3f); // Spawn 3 units away from barracks
                    float3 spawnPosition = barracksPosition + spawnOffset;
                    
                    // Validate final spawn position
                    if (!IsValidPosition(spawnPosition)) {
                        Debug.LogWarning("Calculated spawn position is invalid, using fallback");
                        barracksEntities.Dispose();
                        barracksTransforms.Dispose();
                        return GetSafeFallbackPosition();
                    }
                    
                    barracksEntities.Dispose();
                    barracksTransforms.Dispose();
                    
                    Debug.Log($"Spawning near barracks at position: {spawnPosition}");
                    return spawnPosition;
                }
                
                barracksEntities.Dispose();
                barracksTransforms.Dispose();
            }
            
            // If no barracks found, use the default spawn point or world origin
            float3 fallbackPosition = GetSafeFallbackPosition();
            Debug.Log($"No valid barracks found, using fallback position: {fallbackPosition}");
            return fallbackPosition;
        } catch (System.Exception e) {
            Debug.LogError("Error in FindBarracksSpawnPosition: " + e.Message);
            Debug.LogError("Using fallback spawn position");
            // Use safe fallback position
            return GetSafeFallbackPosition();
        }
    }

    private float3 GetSafeFallbackPosition() {
        // Try to use spawnPoint if it exists and is not null
        if (spawnPoint != null) {
            try {
                Vector3 pos = spawnPoint.position;
                float3 result = new float3(pos.x, pos.y, pos.z);
                
                // Validate that the position contains finite values
                if (IsValidPosition(result)) {
                    return result;
                } else {
                    Debug.LogWarning("spawnPoint position contains invalid values (NaN/Infinity), using fallback");
                }
            } catch (System.Exception e) {
                Debug.LogError("Error accessing spawnPoint position: " + e.Message);
            }
        }
        
        // Try to use this transform as fallback
        if (transform != null) {
            try {
                Vector3 pos = transform.position;
                float3 result = new float3(pos.x, pos.y, pos.z);
                
                // Validate that the position contains finite values
                if (IsValidPosition(result)) {
                    return result;
                } else {
                    Debug.LogWarning("transform position contains invalid values (NaN/Infinity), using world origin");
                }
            } catch (System.Exception e) {
                Debug.LogError("Error accessing transform position: " + e.Message);
            }
        }
        
        // Last resort: use world origin
        Debug.LogWarning("Using world origin (0,0,0) as spawn position");
        return new float3(0, 0, 0);
    }

    private bool IsValidPosition(float3 position) {
        // Check if all components are finite (not NaN or Infinity)
        return math.isfinite(position.x) && math.isfinite(position.y) && math.isfinite(position.z);
    }

    private void SpawnFormation(UnitTypeSO unitTypeSO, EntitiesReferences entitiesReferences, float3 spawnPosition) {
        // Validate spawn position before using it
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