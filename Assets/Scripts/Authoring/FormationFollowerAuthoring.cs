using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Authoring-Komponente für Units, die einer Formation folgen.
/// Diese Units positionieren sich automatisch basierend auf Flag-Bearer Einstellungen.
/// </summary>
public class FormationFollowerAuthoring : MonoBehaviour
{
    [Header("Formation Assignment")]
    [Tooltip("Der Flag-Bearer GameObject, dem diese Unit folgen soll")]
    public GameObject flagBearerGameObject;
    
    [Header("Auto-Assignment")]
    [Tooltip("Automatische Positions-Zuweisung basierend auf Flag-Bearer Formation")]
    public bool autoAssignFormationPosition = true;
    
    [Tooltip("Manuelle Position in der Formation (nur wenn Auto-Assignment deaktiviert)")]
    public int2 manualFormationPosition = new int2(0, 0);
    
    [Header("Movement Settings")]
    [Tooltip("Bewegungsgeschwindigkeit der Unit (überschreibt Flag-Bearer wenn > 0)")]
    public float moveSpeed = 0f; // 0 = verwende Flag-Bearer Speed
    
    [Tooltip("Rotationsgeschwindigkeit der Unit (überschreibt Flag-Bearer wenn > 0)")]
    public float rotationSpeed = 0f; // 0 = verwende Flag-Bearer Speed

    public class Baker : Baker<FormationFollowerAuthoring>
    {
        public override void Bake(FormationFollowerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            // Flag-Bearer Entity referenzieren
            Entity flagBearerEntity = Entity.Null;
            FlagBearerAuthoring flagBearerAuthoring = null;
            
            if (authoring.flagBearerGameObject != null)
            {
                flagBearerEntity = GetEntity(authoring.flagBearerGameObject, TransformUsageFlags.Dynamic);
                flagBearerAuthoring = authoring.flagBearerGameObject.GetComponent<FlagBearerAuthoring>();
            }
            
            // Formation Position bestimmen
            int2 formationPosition;
            float finalMoveSpeed = authoring.moveSpeed;
            float finalRotationSpeed = authoring.rotationSpeed;
            
            if (authoring.autoAssignFormationPosition && flagBearerAuthoring != null)
            {
                // Automatische Zuweisung basierend auf Flag-Bearer Formation
                formationPosition = GetAutoAssignedPosition(authoring, flagBearerAuthoring);
                
                // Verwende Flag-Bearer Speeds wenn nicht überschrieben
                if (finalMoveSpeed <= 0) finalMoveSpeed = flagBearerAuthoring.moveSpeed;
                if (finalRotationSpeed <= 0) finalRotationSpeed = flagBearerAuthoring.rotationSpeed;
            }
            else
            {
                // Manuelle Position verwenden
                formationPosition = authoring.manualFormationPosition;
                
                // Fallback-Werte wenn keine Speeds gesetzt
                if (finalMoveSpeed <= 0) finalMoveSpeed = 5f;
                if (finalRotationSpeed <= 0) finalRotationSpeed = 10f;
            }
            
            // Formation Follower Component hinzufügen
            AddComponent(entity, new FormationFollower
            {
                flagBearerEntity = flagBearerEntity,
                formationPosition = formationPosition,
                targetPosition = float3.zero,
                moveSpeed = finalMoveSpeed,
                rotationSpeed = finalRotationSpeed,
                isMoving = false,
                shouldResetToFormation = false
            });
        }
        
        private int2 GetAutoAssignedPosition(FormationFollowerAuthoring authoring, FlagBearerAuthoring flagBearer)
        {
            // Finde alle Formation-Follower, die dem gleichen Flag-Bearer zugewiesen sind
            var allFollowers = Object.FindObjectsOfType<FormationFollowerAuthoring>();
            var sameFormationFollowers = new System.Collections.Generic.List<FormationFollowerAuthoring>();
            
            foreach (var follower in allFollowers)
            {
                if (follower.flagBearerGameObject == authoring.flagBearerGameObject && 
                    follower.autoAssignFormationPosition)
                {
                    sameFormationFollowers.Add(follower);
                }
            }
            
            // Sortiere nach GameObject-Namen für konsistente Reihenfolge
            sameFormationFollowers.Sort((a, b) => string.Compare(a.name, b.name));
            
            // Finde Index des aktuellen Followers
            int index = sameFormationFollowers.IndexOf(authoring);
            
            // Berechne Position basierend auf Flag-Bearer Formation-Breite
            return FormationUtility.IndexToFormationPosition(index, flagBearer.formationWidth);
        }
    }
    
    // Editor-Hilfsfunktionen
    [ContextMenu("Preview Formation Position")]
    private void PreviewFormationPosition()
    {
        if (flagBearerGameObject == null)
        {
            Debug.LogWarning("Kein Flag-Bearer zugewiesen!");
            return;
        }
        
        var flagBearerAuth = flagBearerGameObject.GetComponent<FlagBearerAuthoring>();
        if (flagBearerAuth == null)
        {
            Debug.LogWarning("Flag-Bearer hat keine FlagBearerAuthoring Komponente!");
            return;
        }
        
        if (autoAssignFormationPosition)
        {
            // Simuliere Auto-Assignment
            var allFollowers = FindObjectsOfType<FormationFollowerAuthoring>();
            var sameFormationFollowers = new System.Collections.Generic.List<FormationFollowerAuthoring>();
            
            foreach (var follower in allFollowers)
            {
                if (follower.flagBearerGameObject == flagBearerGameObject && 
                    follower.autoAssignFormationPosition)
                {
                    sameFormationFollowers.Add(follower);
                }
            }
            
            sameFormationFollowers.Sort((a, b) => string.Compare(a.name, b.name));
            int index = sameFormationFollowers.IndexOf(this);
            int2 pos = FormationUtility.IndexToFormationPosition(index, flagBearerAuth.formationWidth);
            
            Debug.Log($"{name}: Auto-Position = ({pos.x}, {pos.y}), Index = {index}");
        }
        else
        {
            Debug.Log($"{name}: Manuelle Position = ({manualFormationPosition.x}, {manualFormationPosition.y})");
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