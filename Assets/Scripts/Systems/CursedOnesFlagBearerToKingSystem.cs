using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// System das alle Cursed Ones FlagBearer kontinuierlich zur King-Position bewegt.
/// </summary>
partial struct CursedOnesFlagBearerToKingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Keine speziellen Anforderungen
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Finde den King (mit King-Komponente und Carrara Golems Fraktion)
        Entity kingEntity = Entity.Null;
        float3 kingPosition = float3.zero;
        
        foreach (var (localTransform, faction, king, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Faction>, RefRO<King>>().WithEntityAccess())
        {
            if (faction.ValueRO.factionType == FactionType.CarraraGolems)
            {
                // Das ist der Carrara King
                kingEntity = entity;
                kingPosition = localTransform.ValueRO.Position;
                break;
            }
        }
        
        if (kingEntity == Entity.Null)
        {
            // Kein King gefunden
            return;
        }
        
        // Aktualisiere alle Cursed Ones FlagBearer
        foreach (var (flagBearer, faction, entity) in SystemAPI.Query<RefRW<FlagBearer>, RefRO<Faction>>().WithEntityAccess())
        {
            if (faction.ValueRO.factionType == FactionType.CursedOnes)
            {
                // Setze die targetPosition auf die King-Position
                flagBearer.ValueRW.targetPosition = kingPosition;
                
                Debug.Log($"Cursed Ones FlagBearer {entity.Index} bewegt sich zur King-Position: {kingPosition}");
            }
        }
    }
}