using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class RookTacticAuthoring : MonoBehaviour
{
    [Header("Hitbox Position")]
    public Vector3 hitboxOffset = new Vector3(0, 0, 2f); // Relative zur Entität
    
    [Header("Hitbox Size")]
    public float hitboxWidth = 1f;
    public float hitboxHeight = 1f;
    public float hitboxDepth = 1f;
    
    [Header("Visual Settings")]
    public Color hitboxColor = Color.red;
    public bool showRuntimeVisual = true;
    public GameObject hitboxVisualPrefab; // Optional: Custom prefab für die Visualisierung
    
    [Header("Damage Settings")]
    public int damageAmount = 10;
    
    public Vector3 GetHitboxWorldPosition()
    {
        // Transformiere den lokalen Offset in Weltkoordinaten
        return transform.position + transform.TransformDirection(hitboxOffset);
    }
    
    public Vector3 GetHitboxSize()
    {
        return new Vector3(hitboxWidth, hitboxHeight, hitboxDepth);
    }
    
    private void OnDrawGizmos()
    {
        // Zeichne die Hitbox in Gizmos (für Scene View)
        Gizmos.color = hitboxColor;
        
        Vector3 hitboxPosition = GetHitboxWorldPosition();
        Vector3 hitboxSize = GetHitboxSize();
        
        // Setze Gizmo Matrix für Rotation
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(hitboxPosition, transform.rotation, Vector3.one);
        
        // Zeichne Wireframe Cube
        Gizmos.DrawWireCube(Vector3.zero, hitboxSize);
        
        // Zeichne auch eine transparente gefüllte Version
        Color transparentColor = hitboxColor;
        transparentColor.a = 0.2f;
        Gizmos.color = transparentColor;
        Gizmos.DrawCube(Vector3.zero, hitboxSize);
        
        // Stelle Matrix wieder her
        Gizmos.matrix = oldMatrix;
        
        // Zeichne Linie von Entität zur Hitbox
        Gizmos.color = hitboxColor;
        Gizmos.DrawLine(transform.position, hitboxPosition);
        
        // Zeichne Koordinatenachsen an der Hitbox
        Gizmos.color = Color.red;
        Gizmos.DrawLine(hitboxPosition, hitboxPosition + transform.right * 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(hitboxPosition, hitboxPosition + transform.up * 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(hitboxPosition, hitboxPosition + transform.forward * 0.5f);
    }
    
    private void OnDrawGizmosSelected()
    {
        // Zusätzliche Gizmos wenn das Objekt ausgewählt ist
        Gizmos.color = Color.yellow;
        Vector3 hitboxPosition = GetHitboxWorldPosition();
        
        // Zeichne Offset-Vektor
        Gizmos.DrawLine(transform.position, hitboxPosition);
        Gizmos.DrawWireSphere(hitboxPosition, 0.1f);
        
        // Zeige Offset-Werte als Text (nur im Editor)
        #if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(hitboxPosition + Vector3.up * 0.5f, 
            $"Offset: {hitboxOffset}\nSize: {hitboxWidth:F1}x{hitboxHeight:F1}x{hitboxDepth:F1}");
        #endif
    }

    public class Baker : Baker<RookTacticAuthoring>
    {
        public override void Bake(RookTacticAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new RookTactic
            {
                hitboxOffset = authoring.hitboxOffset,
                hitboxWidth = authoring.hitboxWidth,
                hitboxHeight = authoring.hitboxHeight,
                hitboxDepth = authoring.hitboxDepth,
                hitboxColor = new float4(authoring.hitboxColor.r, authoring.hitboxColor.g, authoring.hitboxColor.b, authoring.hitboxColor.a),
                showRuntimeVisual = authoring.showRuntimeVisual,
                timer = 0f,
                timerMax = 1f,
                damageAmount = authoring.damageAmount
            });
            
            // Wenn ein Custom Prefab gesetzt ist, konvertiere es auch
            if (authoring.hitboxVisualPrefab != null)
            {
                Entity visualPrefab = GetEntity(authoring.hitboxVisualPrefab, TransformUsageFlags.Dynamic);
                AddComponent(entity, new RookTacticVisualPrefab
                {
                    prefab = visualPrefab
                });
            }
        }
    }
}

public struct RookTactic : IComponentData
{
    public float3 hitboxOffset;
    public float hitboxWidth;
    public float hitboxHeight;
    public float hitboxDepth;
    public float4 hitboxColor;
    public bool showRuntimeVisual;
    public float timer;
    public float timerMax;
    public int damageAmount;
}

// Allgemeine Kollisionsstatus-Komponente für alle Tactics
public struct TacticCollisionState : IComponentData
{
    public bool isCollidingWithFlagBearer;
    public float4 originalColor;
    public float4 collisionColor; // Grün für FlagBearer-Kollision
}

// Legacy-Komponenten für Rückwärtskompatibilität - verwende stattdessen die generischen Komponenten
public struct RookTacticVisualPrefab : IComponentData
{
    public Entity prefab;
}

public struct RookTacticVisualInstance : IComponentData
{
    public Entity visualEntity;
}

// Deprecated: Verwende stattdessen TacticCollisionState
public struct RookTacticCollisionState : IComponentData
{
    public bool isCollidingWithFlagBearer;
    public float4 originalColor;
    public float4 collisionColor; // Grün für FlagBearer-Kollision
}