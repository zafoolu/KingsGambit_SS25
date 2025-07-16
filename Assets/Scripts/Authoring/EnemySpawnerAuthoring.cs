using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// MonoBehaviour für Enemy-Spawner-Konfiguration im Editor
/// </summary>
public class EnemySpawnerAuthoring : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private UnitTypeSO.UnitType unitType = UnitTypeSO.UnitType.CursedPawn;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int maxSpawns = 0; // 0 = unendlich
    [SerializeField] private Vector3 rallyPositionOffset = new Vector3(0, 0, 5);
    
    [Header("Debug")]
    [SerializeField] private bool showSpawnArea = true;
    [SerializeField] private bool showRallyPosition = true;
    
    private void OnDrawGizmos()
    {
        if (!showSpawnArea && !showRallyPosition) return;
        
        Vector3 position = transform.position;
        
        if (showSpawnArea)
        {
            // Spawn-Position anzeigen
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, 0.5f);
            Gizmos.DrawWireCube(position, Vector3.one * 0.3f);
        }
        
        if (showRallyPosition)
        {
            // Rally-Position anzeigen
            Vector3 rallyPos = position + rallyPositionOffset;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(rallyPos, 0.3f);
            Gizmos.DrawLine(position, rallyPos);
            
            // Rally-Position Label
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(rallyPos + Vector3.up * 0.5f, "Rally Point");
            #endif
        }
    }
    
    private void OnValidate()
    {
        spawnInterval = Mathf.Max(0.1f, spawnInterval);
        maxSpawns = Mathf.Max(0, maxSpawns);
    }
    
    public class Baker : Baker<EnemySpawnerAuthoring>
    {
        public override void Bake(EnemySpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new EnemySpawner
            {
                unitType = authoring.unitType,
                spawnInterval = authoring.spawnInterval,
                maxSpawns = authoring.maxSpawns,
                rallyPositionOffset = authoring.rallyPositionOffset,
                currentSpawnTimer = authoring.spawnInterval,
                currentSpawnCount = 0,
                isActive = true
            });
        }
    }
}

/// <summary>
/// ECS-Komponente für automatisches Enemy-Spawning
/// </summary>
public struct EnemySpawner : IComponentData
{
    public UnitTypeSO.UnitType unitType;
    public float spawnInterval;
    public int maxSpawns;
    public float3 rallyPositionOffset;
    
    // Runtime-Daten
    public float currentSpawnTimer;
    public int currentSpawnCount;
    public bool isActive;
}