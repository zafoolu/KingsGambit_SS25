using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections;

public class KnightTacticAuthoring : MonoBehaviour
{
    [Header("Tactic Box GameObjects")]
    [Tooltip("Ziehe hier die TacticHitbox1 GameObject rein")]
    public GameObject tacticBox1GameObject;
    
    [Tooltip("Ziehe hier die TacticHitbox2 GameObject rein")]
    public GameObject tacticBox2GameObject;

    [Header("Tactic Einstellungen")]
    public float timerMax = 1f;
    public int damageAmount = 10;
    
    [Header("Visual Feedback")]
    [Tooltip("Wie oft pro Sekunde soll das visuelle Feedback aktualisiert werden")]
    public float visualUpdateRate = 10f;
    
    // Private Felder für Material-Management
    private Material box1OriginalMaterial;
    private Material box2OriginalMaterial;
    private Material box1CurrentMaterial;
    private Material box2CurrentMaterial;
    private MeshRenderer box1Renderer;
    private MeshRenderer box2Renderer;
    
    // Materialien für verschiedene Zustände
    private Material goodStateMaterial;  // Grün und transparent
    private Material badStateMaterial;   // Rötlich und transparent
    
    private void Start()
    {
        InitializeVisualFeedback();
        StartCoroutine(UpdateVisualFeedback());
    }
    
    private void InitializeVisualFeedback()
    {
        // Hole MeshRenderer Komponenten
        if (tacticBox1GameObject != null)
        {
            box1Renderer = tacticBox1GameObject.GetComponent<MeshRenderer>();
            if (box1Renderer != null)
            {
                box1OriginalMaterial = box1Renderer.material;
            }
        }
        
        if (tacticBox2GameObject != null)
        {
            box2Renderer = tacticBox2GameObject.GetComponent<MeshRenderer>();
            if (box2Renderer != null)
            {
                box2OriginalMaterial = box2Renderer.material;
            }
        }
        
        // Erstelle Materialien für verschiedene Zustände
        CreateFeedbackMaterials();
    }
    
    private void CreateFeedbackMaterials()
    {
        // Erstelle grünes transparentes Material (genau eine Unit)
        goodStateMaterial = new Material(Shader.Find("Standard"));
        goodStateMaterial.color = new Color(0f, 1f, 0f, 0.3f); // Grün, 30% Transparenz
        goodStateMaterial.SetFloat("_Mode", 3); // Transparent mode
        goodStateMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        goodStateMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        goodStateMaterial.SetInt("_ZWrite", 0);
        goodStateMaterial.DisableKeyword("_ALPHATEST_ON");
        goodStateMaterial.EnableKeyword("_ALPHABLEND_ON");
        goodStateMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        goodStateMaterial.renderQueue = 3000;
        
        // Erstelle rötliches transparentes Material (keine oder mehr als eine Unit)
        badStateMaterial = new Material(Shader.Find("Standard"));
        badStateMaterial.color = new Color(1f, 0.3f, 0.3f, 0.3f); // Rötlich, 30% Transparenz
        badStateMaterial.SetFloat("_Mode", 3); // Transparent mode
        badStateMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        badStateMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        badStateMaterial.SetInt("_ZWrite", 0);
        badStateMaterial.DisableKeyword("_ALPHATEST_ON");
        badStateMaterial.EnableKeyword("_ALPHABLEND_ON");
        badStateMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        badStateMaterial.renderQueue = 3000;
    }
    
    private IEnumerator UpdateVisualFeedback()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f / visualUpdateRate);
            
            // Aktualisiere visuelle Darstellung für beide Boxen
            UpdateBoxVisual(tacticBox1GameObject, box1Renderer);
            UpdateBoxVisual(tacticBox2GameObject, box2Renderer);
        }
    }
    
    private void UpdateBoxVisual(GameObject boxObject, MeshRenderer renderer)
    {
        if (boxObject == null || renderer == null) return;
        
        var boxCollider = boxObject.GetComponent<BoxCollider>();
        if (boxCollider == null) return;
        
        // Zähle Units in der Box
        int unitsInBox = CountUnitsInBox(boxObject.transform, boxCollider);
        
        // Wähle das passende Material basierend auf der Unit-Anzahl
        Material targetMaterial = (unitsInBox == 1) ? goodStateMaterial : badStateMaterial;
        
        // Setze das Material nur wenn es sich geändert hat
        if (renderer.material != targetMaterial)
        {
            renderer.material = targetMaterial;
        }
    }
    
    private void OnDestroy()
    {
        // Cleanup: Zerstöre erstellte Materialien
        if (goodStateMaterial != null)
        {
            DestroyImmediate(goodStateMaterial);
        }
        if (badStateMaterial != null)
        {
            DestroyImmediate(badStateMaterial);
        }
    }

    public class Baker : Baker<KnightTacticAuthoring>
    {
        public override void Bake(KnightTacticAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            Entity box1Entity = Entity.Null;
            Entity box2Entity = Entity.Null;
            
            if (authoring.tacticBox1GameObject != null)
            {
                box1Entity = GetEntity(authoring.tacticBox1GameObject, TransformUsageFlags.Dynamic);
                Debug.Log($"Baked tacticBox1: {authoring.tacticBox1GameObject.name} -> Entity {box1Entity.Index}");
            }
            else
            {
                Debug.LogWarning($"tacticBox1GameObject is null in {authoring.name}!");
            }
            
            if (authoring.tacticBox2GameObject != null)
            {
                box2Entity = GetEntity(authoring.tacticBox2GameObject, TransformUsageFlags.Dynamic);
                Debug.Log($"Baked tacticBox2: {authoring.tacticBox2GameObject.name} -> Entity {box2Entity.Index}");
            }
            else
            {
                Debug.LogWarning($"tacticBox2GameObject is null in {authoring.name}!");
            }
            
            AddComponent(entity, new KnightTactic
            {
                tacticBox1Entity = box1Entity,
                tacticBox2Entity = box2Entity,
                timerMax = authoring.timerMax,
                timer = 0f,
                damageAmount = authoring.damageAmount,
                onShoot = new KnightTactic.OnShootEvent { isTriggered = false }
            });
        }
    }

    private void OnDrawGizmos()
    {
        // Die Boxen sind bereits visuell da, aber wir können trotzdem Gizmos für Debug-Info zeigen
        if (tacticBox1GameObject != null)
        {
            DrawDebugInfo(tacticBox1GameObject, Color.red);
            DrawDOTSHitbox(tacticBox1GameObject, Color.yellow);
        }
        
        if (tacticBox2GameObject != null)
        {
            DrawDebugInfo(tacticBox2GameObject, Color.blue);
            DrawDOTSHitbox(tacticBox2GameObject, Color.cyan);
        }
    }
    
    private void DrawDebugInfo(GameObject boxObject, Color color)
    {
        var boxCollider = boxObject.GetComponent<BoxCollider>();
        if (boxCollider == null) return;
        
        // Zähle Units für Debug-Info
        int unitsInBox = CountUnitsInBox(boxObject.transform, boxCollider);
        
        // Zeige Text über der Box
        Vector3 textPos = boxObject.transform.position + Vector3.up * 2f;
        
#if UNITY_EDITOR
        // Zeichne Prefab-Hitbox (grün = BoxCollider bounds)
        UnityEditor.Handles.color = Color.green;
        Vector3 boxSize = Vector3.Scale(boxCollider.size, boxObject.transform.lossyScale);
        Vector3 boxCenter = boxObject.transform.position + boxObject.transform.TransformVector(boxCollider.center);
        
        // Zeichne Wireframe Cube für Prefab-Hitbox
        Matrix4x4 oldMatrix = UnityEditor.Handles.matrix;
        UnityEditor.Handles.matrix = Matrix4x4.TRS(boxCenter, boxObject.transform.rotation, boxSize);
        UnityEditor.Handles.DrawWireCube(Vector3.zero, Vector3.one);
        UnityEditor.Handles.matrix = oldMatrix;
        
        UnityEditor.Handles.color = (unitsInBox == 1) ? Color.green : color;
        UnityEditor.Handles.Label(textPos, $"Prefab Units: {unitsInBox}");
#endif
    }
    
    private void DrawDOTSHitbox(GameObject boxObject, Color color)
    {
    #if UNITY_EDITOR
        // Zeichne DOTS-berechnete Hitbox (wie im KnightTacticSystem berechnet)
        Transform boxTransform = boxObject.transform;
        
        // Verwende die gleiche Berechnung wie im DOTS System
        Vector3 boxSize = boxTransform.lossyScale; // DOTS verwendet direkt die Scale
        Vector3 boxCenter = boxTransform.position; // DOTS verwendet direkt die Position
        
        // Zeichne DOTS-Hitbox in anderer Farbe
        UnityEditor.Handles.color = color;
        
        // Zeichne Wireframe Cube für DOTS-Hitbox
        Matrix4x4 oldMatrix = UnityEditor.Handles.matrix;
        UnityEditor.Handles.matrix = Matrix4x4.TRS(boxCenter, boxTransform.rotation, boxSize);
        UnityEditor.Handles.DrawWireCube(Vector3.zero, Vector3.one);
        UnityEditor.Handles.matrix = oldMatrix;
        
        // Zeige DOTS-Info
        Vector3 textPos = boxCenter + Vector3.up * 3f;
        UnityEditor.Handles.Label(textPos, $"DOTS Box: {boxSize.x:F1}x{boxSize.y:F1}x{boxSize.z:F1}");
        
        // Zeichne Min/Max Punkte der AABB
        Vector3 aabbMin = boxCenter - boxSize * 0.5f;
        Vector3 aabbMax = boxCenter + boxSize * 0.5f;
        
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireCube(aabbMin, Vector3.one * 0.1f);
        UnityEditor.Handles.Label(aabbMin + Vector3.up * 0.5f, "AABB Min");
        
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.DrawWireCube(aabbMax, Vector3.one * 0.1f);
        UnityEditor.Handles.Label(aabbMax + Vector3.up * 0.5f, "AABB Max");
    #endif
    }
    
    private int CountUnitsInBox(Transform boxTransform, BoxCollider boxCollider)
    {
        Vector3 boxSize = Vector3.Scale(boxCollider.size, boxTransform.lossyScale);
        
        Collider[] colliders = Physics.OverlapBox(
            boxTransform.position + boxTransform.TransformVector(boxCollider.center),
            boxSize / 2,
            boxTransform.rotation,
            LayerMask.GetMask("Unit")
        );

        return colliders?.Length ?? 0;
    }
}

public struct KnightTactic : IComponentData
{
    public Entity tacticBox1Entity;
    public Entity tacticBox2Entity;
    
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