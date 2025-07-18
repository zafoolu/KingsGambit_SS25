using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct PatrolData : IComponentData
{
    public float3 pointA;
    public float3 pointB;
    public float3 pointC;
    public float3 pointD;
    public float speed;
    public float waitTime;
    public int currentTargetIndex; // 0=A, 1=B, 2=C, 3=D
    public float currentWaitTime;
    public bool hasReachedDestination;
    public bool oneWayOnly;
}

public class PatrolAuthoring : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform pointA;
    public Transform pointB;
    public Transform pointC;
    public Transform pointD;
    
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
            if (authoring.pointA == null || authoring.pointB == null || 
                authoring.pointC == null || authoring.pointD == null)
            {
                Debug.LogWarning($"PatrolAuthoring auf {authoring.name}: Nicht alle Patrol-Punkte sind gesetzt! Verwende Fallback-Positionen.");
            }
            
            Vector3 currentPos = authoring.transform.position;
            Vector3 posA = authoring.pointA != null ? authoring.pointA.position : currentPos;
            Vector3 posB = authoring.pointB != null ? authoring.pointB.position : currentPos + Vector3.forward * 5f;
            Vector3 posC = authoring.pointC != null ? authoring.pointC.position : currentPos + Vector3.forward * 10f;
            Vector3 posD = authoring.pointD != null ? authoring.pointD.position : currentPos + Vector3.back * 5f;
            
            // Debug-Ausgabe der Positionen
            Debug.Log($"PatrolAuthoring auf {authoring.name}: Point A = {posA}, Point B = {posB}, Point C = {posC}, Point D = {posD}, Current = {currentPos}");
            
            AddComponent(entity, new PatrolData
            {
                pointA = posA,
                pointB = posB,
                pointC = posC,
                pointD = posD,
                speed = authoring.speed,
                waitTime = authoring.waitTime,
                currentTargetIndex = 1, // Starte mit Bewegung zu Point B
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
        Vector3 posB = pointB != null ? pointB.position : transform.position + Vector3.forward * 5f;
        Vector3 posC = pointC != null ? pointC.position : transform.position + Vector3.forward * 10f;
        Vector3 posD = pointD != null ? pointD.position : transform.position + Vector3.back * 5f;
        
        // Zeichne Patrol-Punkte mit verschiedenen Farben
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(posA, debugRadius);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(posB, debugRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(posC, debugRadius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(posD, debugRadius);
        
        // Zeichne Verbindungslinien zwischen den Punkten (A->B->C->D->A)
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(posA, posB);
        Gizmos.DrawLine(posB, posC);
        Gizmos.DrawLine(posC, posD);
        if (!oneWayOnly)
        {
            Gizmos.DrawLine(posD, posA); // Schließe den Kreis nur wenn nicht One-Way
        }
        
        // Zeichne Richtungspfeile
        DrawArrow(posA, posB);
        DrawArrow(posB, posC);
        DrawArrow(posC, posD);
        if (!oneWayOnly)
        {
            DrawArrow(posD, posA);
        }
    }
    
    private void DrawArrow(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        Vector3 arrowPos = Vector3.Lerp(from, to, 0.7f);
        Vector3 arrowHead1 = arrowPos - direction * 0.3f + Vector3.Cross(direction, Vector3.up) * 0.2f;
        Vector3 arrowHead2 = arrowPos - direction * 0.3f - Vector3.Cross(direction, Vector3.up) * 0.2f;
        
        Gizmos.DrawLine(arrowPos, arrowHead1);
        Gizmos.DrawLine(arrowPos, arrowHead2);
    }
}