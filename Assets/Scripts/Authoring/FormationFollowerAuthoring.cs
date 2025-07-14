using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Authoring-Komponente für Units, die einer Formation folgen.
/// Diese Units positionieren sich relativ zu ihrem zugewiesenen Flag-Bearer.
/// </summary>
public class FormationFollowerAuthoring : MonoBehaviour
{
    [Header("Formation Assignment")]
    [Tooltip("Der Flag-Bearer GameObject, dem diese Unit folgen soll")]
    public GameObject flagBearerGameObject;
    
    [Tooltip("Position in der Formation (X = Spalte, Y = Reihe)")]
    public int2 formationPosition = new int2(0, 0);
    
    [Header("Movement Settings")]
    [Tooltip("Bewegungsgeschwindigkeit der Unit")]
    public float moveSpeed = 5f;
    
    [Tooltip("Rotationsgeschwindigkeit der Unit")]
    public float rotationSpeed = 10f;

    public class Baker : Baker<FormationFollowerAuthoring>
    {
        public override void Bake(FormationFollowerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            // Flag-Bearer Entity referenzieren
            Entity flagBearerEntity = Entity.Null;
            if (authoring.flagBearerGameObject != null)
            {
                flagBearerEntity = GetEntity(authoring.flagBearerGameObject, TransformUsageFlags.Dynamic);
            }
            
            // Formation Follower Component hinzufügen
            AddComponent(entity, new FormationFollower
            {
                flagBearerEntity = flagBearerEntity,
                formationPosition = authoring.formationPosition,
                targetPosition = float3.zero,
                moveSpeed = authoring.moveSpeed,
                rotationSpeed = authoring.rotationSpeed,
                isMoving = false,
                shouldResetToFormation = false
            });
        }
    }
}

/// <summary>
/// Component für Units, die einer Formation folgen.
/// Enthält Referenz zum Flag-Bearer und Formations-Position.
/// </summary>
public struct FormationFollower : IComponentData
{
    // Formation Assignment
    public Entity flagBearerEntity;
    public int2 formationPosition; // X = Spalte, Y = Reihe in der Formation
    
    // Movement
    public float3 targetPosition;
    public float moveSpeed;
    public float rotationSpeed;
    public bool isMoving;
    
    // Formation Reset
    public bool shouldResetToFormation; // Flag um Formation zurückzusetzen wenn kein Target
}