using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Authoring-Komponente für die einfache Erstellung einer kompletten Formation im Editor.
/// Erstellt automatisch einen Flag-Bearer und weist Follower-Units zu.
/// </summary>
public class FormationGroupAuthoring : MonoBehaviour
{
    [Header("Formation Setup")]
    [Tooltip("Der Flag-Bearer für diese Formation")]
    public GameObject flagBearerPrefab;
    
    [Tooltip("Die Unit-Prefabs, die der Formation folgen sollen")]
    public GameObject[] followerPrefabs;
    
    [Header("Formation Settings")]
    [Tooltip("Breite der Formation (automatisch berechnet wenn 0)")]
    public int formationWidth = 0;
    
    [Tooltip("Abstand zwischen Units in der Formation")]
    public float unitSpacing = 2f;
    
    [Tooltip("Abstand der Formation hinter dem Flag-Bearer")]
    public float formationDistance = 3f;
    
    [Header("Movement Settings")]
    [Tooltip("Bewegungsgeschwindigkeit für alle Units")]
    public float moveSpeed = 5f;
    
    [Tooltip("Rotationsgeschwindigkeit für alle Units")]
    public float rotationSpeed = 10f;
    
    [Header("Preview")]
    [Tooltip("Zeige Formation-Positionen im Scene-View")]
    public bool showFormationPreview = true;
    
    [Tooltip("Farbe für Formation-Preview")]
    public Color previewColor = Color.yellow;

    public class Baker : Baker<FormationGroupAuthoring>
    {
        public override void Bake(FormationGroupAuthoring authoring)
        {
            if (authoring.flagBearerPrefab == null || authoring.followerPrefabs == null || authoring.followerPrefabs.Length == 0)
            {
                return;
            }
            
            // Berechne Formation-Parameter
            int unitCount = authoring.followerPrefabs.Length;
            int formationWidth = authoring.formationWidth > 0 ? authoring.formationWidth : FormationUtility.CalculateOptimalFormationWidth(unitCount);
            int formationHeight = FormationUtility.CalculateFormationHeight(unitCount, formationWidth);
            
            // Erstelle Flag-Bearer Entity
            Entity flagBearerEntity = GetEntity(authoring.flagBearerPrefab, TransformUsageFlags.Dynamic);
            
            // Füge Flag-Bearer Component hinzu
            AddComponent(flagBearerEntity, new FlagBearer
            {
                formationWidth = formationWidth,
                formationHeight = formationHeight,
                unitSpacing = authoring.unitSpacing,
                formationDistance = authoring.formationDistance,
                moveSpeed = authoring.moveSpeed,
                rotationSpeed = authoring.rotationSpeed,
                targetPosition = float3.zero,
                isMoving = false
            });
            
            // Erstelle Follower Entities
            for (int i = 0; i < authoring.followerPrefabs.Length; i++)
            {
                if (authoring.followerPrefabs[i] == null) continue;
                
                Entity followerEntity = GetEntity(authoring.followerPrefabs[i], TransformUsageFlags.Dynamic);
                
                // Berechne Formation-Position
                int2 formationPos = FormationUtility.IndexToFormationPosition(i, formationWidth);
                
                // Füge Formation-Follower Component hinzu
                AddComponent(followerEntity, new FormationFollower
                {
                    flagBearerEntity = flagBearerEntity,
                    formationPosition = formationPos,
                    targetPosition = float3.zero,
                    moveSpeed = authoring.moveSpeed,
                    rotationSpeed = authoring.rotationSpeed,
                    isMoving = false
                });
            }
        }
    }
    
    // Editor-Preview Funktionalität
    private void OnDrawGizmos()
    {
        if (!showFormationPreview || followerPrefabs == null || followerPrefabs.Length == 0)
        {
            return;
        }
        
        DrawFormationPreview();
    }
    
    private void DrawFormationPreview()
    {
        // Berechne Formation-Parameter
        int unitCount = followerPrefabs.Length;
        int formationWidth = this.formationWidth > 0 ? this.formationWidth : FormationUtility.CalculateOptimalFormationWidth(unitCount);
        
        // Flag-Bearer Position
        Vector3 flagBearerPos = transform.position;
        
        // Zeichne Flag-Bearer
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(flagBearerPos, 0.5f);
        Gizmos.DrawLine(flagBearerPos, flagBearerPos + transform.forward * 2f);
        
        // Zeichne Formation-Positionen
        Gizmos.color = previewColor;
        
        for (int i = 0; i < unitCount; i++)
        {
            int2 formationPos = FormationUtility.IndexToFormationPosition(i, formationWidth);
            
            float3 worldPos = FormationUtility.CalculateWorldFormationPosition(
                (float3)flagBearerPos,
                transform.rotation,
                formationPos,
                formationWidth,
                unitSpacing,
                formationDistance
            );
            
            // Zeichne Unit-Position
            Gizmos.DrawWireSphere(worldPos, 0.3f);
            
            // Zeichne Verbindungslinie zum Flag-Bearer
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(flagBearerPos, worldPos);
            Gizmos.color = previewColor;
            
            // Zeichne Formation-Index
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(worldPos + (float3)Vector3.up, i.ToString());
            #endif
        }
    }
    
    // Editor-Hilfsfunktionen
    [ContextMenu("Auto-Assign Formation Positions")]
    private void AutoAssignFormationPositions()
    {
        if (followerPrefabs == null || followerPrefabs.Length == 0)
        {
            Debug.LogWarning("Keine Follower-Prefabs zugewiesen!");
            return;
        }
        
        // Berechne optimale Formation-Breite
        if (formationWidth <= 0)
        {
            formationWidth = FormationUtility.CalculateOptimalFormationWidth(followerPrefabs.Length);
            Debug.Log($"Formation-Breite automatisch auf {formationWidth} gesetzt.");
        }
    }
    
    [ContextMenu("Validate Formation Setup")]
    private void ValidateFormationSetup()
    {
        if (flagBearerPrefab == null)
        {
            Debug.LogError("Flag-Bearer Prefab fehlt!");
            return;
        }
        
        if (followerPrefabs == null || followerPrefabs.Length == 0)
        {
            Debug.LogError("Keine Follower-Prefabs zugewiesen!");
            return;
        }
        
        int nullCount = 0;
        for (int i = 0; i < followerPrefabs.Length; i++)
        {
            if (followerPrefabs[i] == null) nullCount++;
        }
        
        if (nullCount > 0)
        {
            Debug.LogWarning($"{nullCount} Follower-Prefab Slots sind leer!");
        }
        
        Debug.Log($"Formation Setup validiert: {followerPrefabs.Length - nullCount} Units, Breite: {(formationWidth > 0 ? formationWidth : FormationUtility.CalculateOptimalFormationWidth(followerPrefabs.Length))}");
    }
}