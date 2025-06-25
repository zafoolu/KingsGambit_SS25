using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct CapturePointSystem : ISystem {
    public void OnUpdate(ref SystemState state) {
        foreach (var (capturePoint, transform) in 
            SystemAPI.Query<RefRW<CapturePoint>, RefRO<LocalTransform>>()) {

            bool unitsInRange = false;
            FactionType capturingFaction = FactionType.None;

            // Überprüfe alle Units in der Nähe
            foreach (var (unitTransform, faction) in 
                SystemAPI.Query<RefRO<LocalTransform>, RefRO<Faction>>()) {
                
                float3 capturePos = transform.ValueRO.Position;
                float3 unitPos = unitTransform.ValueRO.Position;
                float distanceSqr = math.distancesq(capturePos, unitPos);

                if (distanceSqr <= capturePoint.ValueRO.radius * capturePoint.ValueRO.radius) {
                    unitsInRange = true;
                    capturingFaction = faction.ValueRO.factionType;
                    break;
                }
            }

            if (unitsInRange) {
                if (capturingFaction != capturePoint.ValueRO.controllingFaction) {
                    capturePoint.ValueRW.currentCaptureTime += SystemAPI.Time.DeltaTime;
                    
                    // Zuerst auf neutral setzen
                    if (capturePoint.ValueRO.currentCaptureTime >= 5f && capturePoint.ValueRO.controllingFaction != FactionType.None) {
                        capturePoint.ValueRW.controllingFaction = FactionType.None;
                        capturePoint.ValueRW.currentCaptureTime = 0f;
                        UnityEngine.Debug.Log("[CapturePoint] Zone ist jetzt neutral!");
                    }
                    // Dann von der neuen Fraktion einnehmen lassen
                    else if (capturePoint.ValueRO.currentCaptureTime >= 5f && capturePoint.ValueRO.controllingFaction == FactionType.None) {
                        capturePoint.ValueRW.controllingFaction = capturingFaction;
                        capturePoint.ValueRW.isCaptured = true;
                        UnityEngine.Debug.Log($"[CapturePoint] Eingenommen von {capturingFaction}!");
                    }
                    
                    string phase = capturePoint.ValueRO.controllingFaction == FactionType.None ? "Einnahme" : "Neutralisierung";
                    UnityEngine.Debug.Log($"[CapturePoint] {phase} Fortschritt: {(capturePoint.ValueRO.currentCaptureTime / 5f * 100):F1}% durch {capturingFaction}");
                }
            } else {
                capturePoint.ValueRW.currentCaptureTime = 0f;
            }
        }
    }
}