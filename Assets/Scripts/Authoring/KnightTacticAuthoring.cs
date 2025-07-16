using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class KnightTacticAuthoring : MonoBehaviour
{
    [Header("Hitbox 1 Settings")]
    public Vector3 hitbox1Offset = new Vector3(-1f, 0, 2f); // Relative zur Entität
    public Vector3 hitbox1Rotation = Vector3.zero; // Euler Winkel für Rotation
    public float hitbox1Width = 1f;
    public float hitbox1Height = 1f;
    public float hitbox1Depth = 1f;
    
    [Header("Hitbox 2 Settings")]
    public Vector3 hitbox2Offset = new Vector3(1f, 0, 2f); // Relative zur Entität
    public Vector3 hitbox2Rotation = Vector3.zero; // Euler Winkel für Rotation
    public float hitbox2Width = 1f;
    public float hitbox2Height = 1f;
    public float hitbox2Depth = 1f;
    
    [Header("Visual Settings")]
    public Color hitboxColor = Color.yellow;
    public bool showRuntimeVisual = true;
    public GameObject hitboxVisualPrefab; // Optional: Custom prefab für die Visualisierung
    
    [Header("Tactic Settings")]
    public float timerMax = 1f;
    public int damageAmount = 10;
    
    public Vector3 GetHitbox1WorldPosition()
    {
        // Transformiere den lokalen Offset in Weltkoordinaten
        return transform.position + transform.TransformDirection(hitbox1Offset);
    }
    
    public Quaternion GetHitbox1WorldRotation()
    {
        // Kombiniere Entity-Rotation mit Hitbox-Rotation
        return transform.rotation * Quaternion.Euler(hitbox1Rotation);
    }
    
    public Vector3 GetHitbox1Size()
    {
        return new Vector3(hitbox1Width, hitbox1Height, hitbox1Depth);
    }
    
    public Vector3 GetHitbox2WorldPosition()
    {
        // Transformiere den lokalen Offset in Weltkoordinaten
        return transform.position + transform.TransformDirection(hitbox2Offset);
    }
    
    public Quaternion GetHitbox2WorldRotation()
    {
        // Kombiniere Entity-Rotation mit Hitbox-Rotation
        return transform.rotation * Quaternion.Euler(hitbox2Rotation);
    }
    
    public Vector3 GetHitbox2Size()
    {
        return new Vector3(hitbox2Width, hitbox2Height, hitbox2Depth);
    }
    
    private void OnDrawGizmos()
    {
        // Zeichne beide Hitboxen in Gizmos (für Scene View)
        DrawHitboxGizmo(hitbox1Offset, hitbox1Rotation, GetHitbox1Size(), Color.red, "Box 1");
        DrawHitboxGizmo(hitbox2Offset, hitbox2Rotation, GetHitbox2Size(), Color.blue, "Box 2");
    }
    
    private void DrawHitboxGizmo(Vector3 offset, Vector3 rotation, Vector3 size, Color color, string label)
    {
        Gizmos.color = color;
        
        Vector3 hitboxPosition = transform.position + transform.TransformDirection(offset);
        Quaternion hitboxRotation = transform.rotation * Quaternion.Euler(rotation);
        
        // Setze Gizmo Matrix für Position und Rotation
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(hitboxPosition, hitboxRotation, Vector3.one);
        
        // Zeichne Wireframe Cube
        Gizmos.DrawWireCube(Vector3.zero, size);
        
        // Zeichne auch eine transparente gefüllte Version
        Color transparentColor = color;
        transparentColor.a = 0.2f;
        Gizmos.color = transparentColor;
        Gizmos.DrawCube(Vector3.zero, size);
        
        // Stelle Matrix wieder her
        Gizmos.matrix = oldMatrix;
        
        // Zeichne Linie von Entität zur Hitbox
        Gizmos.color = color;
        Gizmos.DrawLine(transform.position, hitboxPosition);
        
        // Zeichne Koordinatenachsen an der Hitbox (mit individueller Rotation)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(hitboxPosition, hitboxPosition + hitboxRotation * Vector3.right * 0.3f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(hitboxPosition, hitboxPosition + hitboxRotation * Vector3.up * 0.3f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(hitboxPosition, hitboxPosition + hitboxRotation * Vector3.forward * 0.3f);
    }
    
    private void OnDrawGizmosSelected()
    {
        // Zusätzliche Gizmos wenn das Objekt ausgewählt ist
        Gizmos.color = Color.yellow;
        
        Vector3 hitbox1Position = GetHitbox1WorldPosition();
        Vector3 hitbox2Position = GetHitbox2WorldPosition();
        
        // Zeichne Offset-Vektoren
        Gizmos.DrawLine(transform.position, hitbox1Position);
        Gizmos.DrawLine(transform.position, hitbox2Position);
        Gizmos.DrawWireSphere(hitbox1Position, 0.1f);
        Gizmos.DrawWireSphere(hitbox2Position, 0.1f);
        
        // Zeige Offset-Werte als Text (nur im Editor)
        #if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(hitbox1Position + Vector3.up * 0.5f, 
            $"Box 1\nOffset: {hitbox1Offset}\nRotation: {hitbox1Rotation}\nSize: {hitbox1Width:F1}x{hitbox1Height:F1}x{hitbox1Depth:F1}");
        UnityEditor.Handles.Label(hitbox2Position + Vector3.up * 0.5f, 
            $"Box 2\nOffset: {hitbox2Offset}\nRotation: {hitbox2Rotation}\nSize: {hitbox2Width:F1}x{hitbox2Height:F1}x{hitbox2Depth:F1}");
        #endif
    }

    public class Baker : Baker<KnightTacticAuthoring>
    {
        public override void Bake(KnightTacticAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new KnightTactic
            {
                hitbox1Offset = authoring.hitbox1Offset,
                hitbox1Rotation = authoring.hitbox1Rotation,
                hitbox1Width = authoring.hitbox1Width,
                hitbox1Height = authoring.hitbox1Height,
                hitbox1Depth = authoring.hitbox1Depth,
                hitbox2Offset = authoring.hitbox2Offset,
                hitbox2Rotation = authoring.hitbox2Rotation,
                hitbox2Width = authoring.hitbox2Width,
                hitbox2Height = authoring.hitbox2Height,
                hitbox2Depth = authoring.hitbox2Depth,
                hitboxColor = new float4(authoring.hitboxColor.r, authoring.hitboxColor.g, authoring.hitboxColor.b, authoring.hitboxColor.a),
                showRuntimeVisual = authoring.showRuntimeVisual,
                timerMax = authoring.timerMax,
                timer = 0f,
                damageAmount = authoring.damageAmount,
                onShoot = new KnightTactic.OnShootEvent { isTriggered = false }
            });
            
            // Wenn ein Custom Prefab gesetzt ist, konvertiere es auch
            if (authoring.hitboxVisualPrefab != null)
            {
                Entity visualPrefab = GetEntity(authoring.hitboxVisualPrefab, TransformUsageFlags.Dynamic);
                AddComponent(entity, new KnightTacticVisualPrefab
                {
                    prefab = visualPrefab
                });
            }
        }
    }
}

public struct KnightTactic : IComponentData
{
    // Hitbox 1 Settings
    public float3 hitbox1Offset;
    public float3 hitbox1Rotation;
    public float hitbox1Width;
    public float hitbox1Height;
    public float hitbox1Depth;
    
    // Hitbox 2 Settings
    public float3 hitbox2Offset;
    public float3 hitbox2Rotation;
    public float hitbox2Width;
    public float hitbox2Height;
    public float hitbox2Depth;
    
    // Visual Settings
    public float4 hitboxColor;
    public bool showRuntimeVisual;
    
    // Tactic Settings
    public float timer;
    public float timerMax;
    public int damageAmount;
    public OnShootEvent onShoot;

    public struct OnShootEvent
    {
        public bool isTriggered;
        public float3 shootFromPosition;
    }
}

// Visual Prefab Component für Knight Tactics
public struct KnightTacticVisualPrefab : IComponentData
{
    public Entity prefab;
}

public struct KnightTacticVisualInstance : IComponentData
{
    public Entity visual1Entity;
    public Entity visual2Entity;
}