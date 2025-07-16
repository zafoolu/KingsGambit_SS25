using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using System.Collections.Generic;

public class TacticVisualManager : MonoBehaviour
{
    [Header("Visual Settings")]
    public Material hitboxMaterial;
    public bool useWireframe = true;
    
    private EntityManager entityManager;
    private EntityQuery visualQuery;
    private Dictionary<Entity, GameObject> visualGameObjects = new Dictionary<Entity, GameObject>();
    
    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        // Query für alle Visual Marker
        visualQuery = entityManager.CreateEntityQuery(
            ComponentType.ReadOnly<TacticVisualMarker>(),
            ComponentType.ReadOnly<LocalTransform>()
        );
        
        // Erstelle Standard-Material falls keines gesetzt
        if (hitboxMaterial == null)
        {
            CreateDefaultMaterial();
        }
    }
    
    private void CreateDefaultMaterial()
    {
        hitboxMaterial = new Material(Shader.Find("Standard"));
        hitboxMaterial.color = new Color(1f, 0f, 0f, 0.3f);
        hitboxMaterial.SetFloat("_Mode", 3); // Transparent mode
        hitboxMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        hitboxMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        hitboxMaterial.SetInt("_ZWrite", 0);
        hitboxMaterial.DisableKeyword("_ALPHATEST_ON");
        hitboxMaterial.EnableKeyword("_ALPHABLEND_ON");
        hitboxMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        hitboxMaterial.renderQueue = 3000;
    }
    
    private void Update()
    {
        if (!visualQuery.IsEmpty)
        {
            UpdateVisuals();
        }
        
        // Cleanup zerstörte Entities
        CleanupDestroyedVisuals();
    }
    
    private void UpdateVisuals()
    {
        var entities = visualQuery.ToEntityArray(Allocator.Temp);
        var transforms = visualQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        var visualMarkers = visualQuery.ToComponentDataArray<TacticVisualMarker>(Allocator.Temp);
        
        for (int i = 0; i < entities.Length; i++)
        {
            Entity entity = entities[i];
            LocalTransform transform = transforms[i];
            TacticVisualMarker marker = visualMarkers[i];
            
            // Erstelle GameObject falls noch nicht vorhanden
            if (!visualGameObjects.ContainsKey(entity))
            {
                CreateVisualGameObject(entity, marker);
            }
            
            // Update Position, Rotation und Scale
            if (visualGameObjects.TryGetValue(entity, out GameObject visualGO))
            {
                visualGO.transform.position = transform.Position;
                visualGO.transform.rotation = transform.Rotation;
                // Verwende die tatsächlichen Hitbox-Dimensionen aus marker.size
                visualGO.transform.localScale = new Vector3(marker.size.x, marker.size.y, marker.size.z);
                
                // Update Farbe falls geändert 
                UpdateVisualColor(visualGO, marker.color);
            }
        }
        
        entities.Dispose();
        transforms.Dispose();
        visualMarkers.Dispose();
    }
    
    private void CreateVisualGameObject(Entity entity, TacticVisualMarker marker)
    {
        GameObject visualGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visualGO.name = $"Tactic_Visual_{entity.Index}";
        
        // Entferne Collider
        var collider = visualGO.GetComponent<Collider>();
        if (collider != null)
        {
            DestroyImmediate(collider);
        }
        
        // Setze Material
        var renderer = visualGO.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material instanceMaterial = new Material(hitboxMaterial);
            instanceMaterial.color = new Color(marker.color.x, marker.color.y, marker.color.z, marker.color.w * 0.3f);
            renderer.material = instanceMaterial;
        }
        
        // Erstelle Wireframe falls gewünscht
        if (useWireframe)
        {
            CreateWireframe(visualGO, marker.color);
        }
        
        visualGameObjects[entity] = visualGO;
    }
    
    private void CreateWireframe(GameObject parent, float4 color)
    {
        GameObject wireframeParent = new GameObject("Wireframe");
        wireframeParent.transform.SetParent(parent.transform);
        wireframeParent.transform.localPosition = Vector3.zero;
        wireframeParent.transform.localRotation = Quaternion.identity;
        wireframeParent.transform.localScale = Vector3.one;
        
        // Erstelle 12 Linien für einen Würfel
        Vector3[] wireframeLines = new Vector3[]
        {
            // Bottom face
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, -0.5f),
            
            // Top face
            new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
            
            // Vertical lines
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f)
        };
        
        for (int i = 0; i < wireframeLines.Length; i += 2)
        {
            CreateWireframeLine(wireframeParent, wireframeLines[i], wireframeLines[i + 1], color);
        }
    }
    
    private void CreateWireframeLine(GameObject parent, Vector3 start, Vector3 end, float4 color)
    {
        GameObject line = new GameObject("WireframeLine");
        line.transform.SetParent(parent.transform);
        
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(color.x, color.y, color.z, color.w);
        lr.endColor = new Color(color.x, color.y, color.z, color.w);
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.positionCount = 2;
        lr.useWorldSpace = false;
        
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }
    
    private void UpdateVisualColor(GameObject visualGO, float4 color)
    {
        var renderer = visualGO.GetComponent<MeshRenderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.color = new Color(color.x, color.y, color.z, color.w * 0.3f);
        }
    }
    
    private void CleanupDestroyedVisuals()
    {
        var keysToRemove = new List<Entity>();
        
        foreach (var kvp in visualGameObjects)
        {
            if (!entityManager.Exists(kvp.Key))
            {
                if (kvp.Value != null)
                {
                    DestroyImmediate(kvp.Value);
                }
                keysToRemove.Add(kvp.Key);
            }
        }
        
        foreach (var key in keysToRemove)
        {
            visualGameObjects.Remove(key);
        }
    }
    
    private void OnDestroy()
    {
        // Cleanup alle Visual GameObjects
        foreach (var kvp in visualGameObjects)
        {
            if (kvp.Value != null)
            {
                DestroyImmediate(kvp.Value);
            }
        }
        visualGameObjects.Clear();
    }
}