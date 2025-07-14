using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Authoring-Komponente für Flag-Bearer Entities.
/// Ein Flag-Bearer ist die führende Entity einer Formation, die das Ziel für alle Follower-Units bestimmt.
/// </summary>
public class FlagBearerAuthoring : MonoBehaviour
{
    [Header("Formation Settings")]
    [Tooltip("Breite der Formation (Anzahl Units pro Reihe)")]
    public int formationWidth = 3;
    
    [Tooltip("Höhe der Formation (Anzahl Reihen)")]
    public int formationHeight = 3;
    
    [Tooltip("Abstand zwischen Units in der Formation")]
    public float unitSpacing = 2f;
    
    [Tooltip("Abstand der Formation hinter dem Flag-Bearer")]
    public float formationDistance = 3f;

    [Header("Movement Settings")]
    [Tooltip("Bewegungsgeschwindigkeit des Flag-Bearers")]
    public float moveSpeed = 5f;
    
    [Tooltip("Rotationsgeschwindigkeit des Flag-Bearers")]
    public float rotationSpeed = 10f;

    public class Baker : Baker<FlagBearerAuthoring>
    {
        public override void Bake(FlagBearerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            // Flag-Bearer Component hinzufügen
            AddComponent(entity, new FlagBearer
            {
                formationWidth = authoring.formationWidth,
                formationHeight = authoring.formationHeight,
                unitSpacing = authoring.unitSpacing,
                formationDistance = authoring.formationDistance,
                moveSpeed = authoring.moveSpeed,
                rotationSpeed = authoring.rotationSpeed,
                targetPosition = authoring.transform.position,
                isMoving = false
            });
        }
    }
}

/// <summary>
/// Component für Flag-Bearer Entities.
/// Enthält alle Informationen für die Formation und Bewegung.
/// </summary>
public struct FlagBearer : IComponentData
{
    // Formation Settings
    public int formationWidth;
    public int formationHeight;
    public float unitSpacing;
    public float formationDistance;
    
    // Movement Settings
    public float moveSpeed;
    public float rotationSpeed;
    public float3 targetPosition;
    public bool isMoving;
}