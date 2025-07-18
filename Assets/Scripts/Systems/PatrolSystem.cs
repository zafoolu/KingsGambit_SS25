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
            float3 targetPos = GetTargetPosition(patrol.ValueRO);
            
            // Debug-Ausgabe der Positionen
            UnityEngine.Debug.Log($"Patrol Debug - Current: {currentPos}, Target: {targetPos}, TargetIndex: {patrol.ValueRO.currentTargetIndex}");
            
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
                    // One-Way: Prüfe ob wir am letzten Punkt (D = Index 3) angekommen sind
                    if (patrol.ValueRO.currentTargetIndex == 3)
                    {
                        patrol.ValueRW.hasReachedDestination = true;
                    }
                    else
                    {
                        // Gehe zum nächsten Punkt
                        patrol.ValueRW.currentTargetIndex = patrol.ValueRO.currentTargetIndex + 1;
                    }
                }
                else
                {
                    // Normal Patrol: Gehe zum nächsten Punkt im Kreis (A->B->C->D->A)
                    patrol.ValueRW.currentTargetIndex = (patrol.ValueRO.currentTargetIndex + 1) % 4;
                    
                    // Drehe zur neuen Richtung
                    float3 newTargetPos = GetTargetPosition(patrol.ValueRO);
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
    
    private static float3 GetTargetPosition(PatrolData patrol)
    {
        switch (patrol.currentTargetIndex)
        {
            case 0: return patrol.pointA;
            case 1: return patrol.pointB;
            case 2: return patrol.pointC;
            case 3: return patrol.pointD;
            default: return patrol.pointA; // Fallback
        }
    }
}