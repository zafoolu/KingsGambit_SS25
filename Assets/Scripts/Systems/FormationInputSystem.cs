using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// System für die Verarbeitung von Formation-Input.
/// Verarbeitet Mausklicks und setzt Ziele nur für Flag-Bearer, nicht für einzelne Units.
/// </summary>
partial struct FormationInputSystem : ISystem
{
    private ComponentLookup<FlagBearer> flagBearerLookup;
    private ComponentLookup<LocalTransform> localTransformLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        flagBearerLookup = SystemAPI.GetComponentLookup<FlagBearer>(false);
        localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
    }

    public void OnUpdate(ref SystemState state)
    {
        // Complete any pending jobs before accessing ComponentLookup
        state.Dependency.Complete();
        
        // Update Lookups
        flagBearerLookup.Update(ref state);
        localTransformLookup.Update(ref state);

        // Prüfe auf Mausklick (Rechtsklick für Bewegung)
        if (Input.GetMouseButtonDown(1)) // Rechtsklick
        {
            ProcessFormationMoveCommand(ref state);
        }
    }

    /// <summary>
    /// Verarbeitet Bewegungsbefehl für Formationen.
    /// </summary>
    private void ProcessFormationMoveCommand(ref SystemState state)
    {
        // Hole Mausposition in der Welt
        float3 mouseWorldPosition = GetMouseWorldPosition();
        if (math.all(mouseWorldPosition == float3.zero))
        {
            return; // Ungültige Position
        }

        // Finde alle ausgewählten Flag-Bearer
        EntityQuery flagBearerQuery = SystemAPI.QueryBuilder()
            .WithAll<FlagBearer, Selected>()
            .Build();

        if (flagBearerQuery.CalculateEntityCount() == 0)
        {
            return; // Keine Flag-Bearer ausgewählt
        }

        // Setze Zielposition für alle ausgewählten Flag-Bearer
        NativeArray<Entity> flagBearerEntities = flagBearerQuery.ToEntityArray(Allocator.Temp);
        
        for (int i = 0; i < flagBearerEntities.Length; i++)
        {
            Entity flagBearerEntity = flagBearerEntities[i];
            
            if (flagBearerLookup.HasComponent(flagBearerEntity))
            {
                FlagBearer flagBearer = flagBearerLookup[flagBearerEntity];
                
                // Berechne Offset für mehrere Flag-Bearer (falls mehrere ausgewählt)
                float3 targetPosition = mouseWorldPosition;
                if (flagBearerEntities.Length > 1)
                {
                    // Verteile Flag-Bearer in einer Linie
                    float offset = (i - (flagBearerEntities.Length - 1) * 0.5f) * 10f; // 10 Units Abstand
                    targetPosition += new float3(offset, 0, 0);
                }
                
                flagBearer.targetPosition = targetPosition;
                flagBearerLookup[flagBearerEntity] = flagBearer;
            }
        }
        
        flagBearerEntities.Dispose();
    }

    /// <summary>
    /// Konvertiert Mausposition zu Weltposition mittels Raycast.
    /// </summary>
    private float3 GetMouseWorldPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return float3.zero;
        }

        Vector3 mousePosition = Input.mousePosition;
        UnityEngine.Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        
        // Raycast auf Ground-Layer (angenommen Layer 0)
        if (Physics.Raycast(ray, out UnityEngine.RaycastHit hit, Mathf.Infinity, 1 << 0))
        {
            return hit.point;
        }
        
        // Fallback: Raycast auf Y=0 Ebene
        float distance = -ray.origin.y / ray.direction.y;
        if (distance > 0)
        {
            Vector3 worldPosition = ray.origin + ray.direction * distance;
            return new float3(worldPosition.x, 0, worldPosition.z);
        }
        
        return float3.zero;
    }
}