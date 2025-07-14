using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct PatrolSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PatrolData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        
        foreach (var (transform, patrol) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<PatrolData>>())
        {
            // Stoppe wenn Ziel erreicht und One-Way-Modus aktiv
            if (patrol.ValueRO.oneWayOnly && patrol.ValueRO.hasReachedDestination)
            {
                continue;
            }
            
            // Wenn wir warten, reduziere die Wartezeit
            if (patrol.ValueRO.currentWaitTime > 0f)
            {
                patrol.ValueRW.currentWaitTime -= deltaTime;
                continue;
            }
            
            float3 currentPos = transform.ValueRO.Position;
            float3 targetPos = patrol.ValueRO.isMovingToB ? patrol.ValueRO.pointB : patrol.ValueRO.pointA;
            
            // Debug-Ausgabe der Positionen
            UnityEngine.Debug.Log($"Patrol Debug - Current: {currentPos}, Target: {targetPos}, PointA: {patrol.ValueRO.pointA}, PointB: {patrol.ValueRO.pointB}, MovingToB: {patrol.ValueRO.isMovingToB}");
            
            // Überprüfe auf ungültige Zielposition
            if (math.all(targetPos == float3.zero))
            {
                UnityEngine.Debug.LogWarning("Patrol: Zielposition ist (0,0,0)! Überspringe Bewegung.");
                continue;
            }
            
            // Berechne Richtung und Distanz zum Ziel
            float3 direction = math.normalize(targetPos - currentPos);
            float distanceToTarget = math.distance(currentPos, targetPos);
            
            // Überprüfe auf ungültige Richtung
            if (math.any(math.isnan(direction)))
            {
                UnityEngine.Debug.LogWarning($"Patrol: Ungültige Richtung berechnet! Current: {currentPos}, Target: {targetPos}");
                continue;
            }
            
            // Bewege zur Zielposition
            float moveDistance = patrol.ValueRO.speed * deltaTime;
            
            if (distanceToTarget <= moveDistance)
            {
                // Ziel erreicht - setze Position exakt
                transform.ValueRW.Position = targetPos;
                patrol.ValueRW.currentWaitTime = patrol.ValueRO.waitTime;
                
                if (patrol.ValueRO.oneWayOnly)
                {
                    // One-Way: Stoppe hier
                    patrol.ValueRW.hasReachedDestination = true;
                }
                else
                {
                    // Normal Patrol: Wechsle Richtung
                    patrol.ValueRW.isMovingToB = !patrol.ValueRO.isMovingToB;
                    
                    // Drehe zur neuen Richtung
                    float3 newTargetPos = patrol.ValueRO.isMovingToB ? patrol.ValueRO.pointB : patrol.ValueRO.pointA;
                    float3 newDirection = math.normalize(newTargetPos - targetPos);
                    if (math.lengthsq(newDirection) > 0.001f)
                    {
                        transform.ValueRW.Rotation = quaternion.LookRotationSafe(newDirection, math.up());
                    }
                }
            }
            else
            {
                // Bewege in Richtung Ziel
                transform.ValueRW.Position = currentPos + direction * moveDistance;
                
                // Drehe in Bewegungsrichtung
                if (math.lengthsq(direction) > 0.001f)
                {
                    transform.ValueRW.Rotation = quaternion.LookRotationSafe(direction, math.up());
                }
            }
        }
    }
}