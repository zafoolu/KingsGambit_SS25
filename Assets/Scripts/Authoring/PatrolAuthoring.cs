using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct PatrolData : IComponentData
{
    public float3 pointA;
    public float3 pointB;
    public float speed;
    public float waitTime;
    public bool isMovingToB;
    public float currentWaitTime;
    public bool hasReachedDestination;
    public bool oneWayOnly;
}

public class PatrolAuthoring : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform pointA;
    public Transform pointB;
    
    [Range(1f, 10f)]
    public float speed = 3f;
    
    [Range(0f, 5f)]
    public float waitTime = 1f;
    
    [Header("Movement Type")]
    public bool oneWayOnly = false;
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    public float debugRadius = 0.5f;
    
    private class Baker : Baker<PatrolAuthoring>
    {
        public override void Bake(PatrolAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            // Warnung wenn Patrol-Punkte nicht gesetzt sind
            if (authoring.pointA == null || authoring.pointB == null)
            {
                Debug.LogWarning($"PatrolAuthoring auf {authoring.name}: Point A oder Point B ist nicht gesetzt! Verwende Fallback-Positionen.");
            }
            
            Vector3 currentPos = authoring.transform.position;
            Vector3 posA = authoring.pointA != null ? authoring.pointA.position : currentPos;
            Vector3 posB = authoring.pointB != null ? authoring.pointB.position : currentPos + Vector3.forward * 10f;
            
            // Debug-Ausgabe der Positionen
            Debug.Log($"PatrolAuthoring auf {authoring.name}: Point A = {posA}, Point B = {posB}, Current = {currentPos}");
            
            AddComponent(entity, new PatrolData
            {
                pointA = posA,
                pointB = posB,
                speed = authoring.speed,
                waitTime = authoring.waitTime,
                isMovingToB = true,
                currentWaitTime = 0f,
                hasReachedDestination = false,
                oneWayOnly = authoring.oneWayOnly
            });
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        Vector3 posA = pointA != null ? pointA.position : transform.position;
        Vector3 posB = pointB != null ? pointB.position : transform.position + Vector3.forward * 10f;
        
        // Zeichne Patrol-Punkte
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(posA, debugRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(posB, debugRadius);
        
        // Zeichne Verbindungslinie
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(posA, posB);
        
        // Zeichne Richtungspfeil
        Vector3 direction = (posB - posA).normalized;
        Vector3 arrowHead1 = posB - direction * 0.5f + Vector3.Cross(direction, Vector3.up) * 0.3f;
        Vector3 arrowHead2 = posB - direction * 0.5f - Vector3.Cross(direction, Vector3.up) * 0.3f;
        
        Gizmos.DrawLine(posB, arrowHead1);
        Gizmos.DrawLine(posB, arrowHead2);
    }
}