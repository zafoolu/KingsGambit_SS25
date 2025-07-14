using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Einfaches System: Alle Cursed Einheiten laufen zum King.
/// </summary>
partial struct EnemyTargetKingSystem : ISystem
{
    private bool hasRun;
    
    public void OnCreate(ref SystemState state)
    {
        hasRun = false;
    }
    
    public void OnUpdate(ref SystemState state)
    {
        // Nur einmal ausführen
        if (hasRun) return;
        
        // Finde Carrara King Entity
        Entity kingEntity = Entity.Null;
        int kingCount = 0;
        
        foreach (var (unitTypeHolder, entity) in SystemAPI.Query<RefRO<UnitTypeHolder>>().WithEntityAccess())
        {
            if (unitTypeHolder.ValueRO.unitType == UnitTypeSO.UnitType.CarraraKing)
            {
                kingEntity = entity;
                kingCount++;
                Debug.Log($"Carrara King gefunden: {entity}");
                break;
            }
        }
        
        if (kingEntity == Entity.Null)
        {
            Debug.Log("Kein Carrara King gefunden!");
            return;
        }
        
        // Alle Cursed Einheiten zum King schicken
        int cursedCount = 0;
        foreach (var (unitTypeHolder, entity) in SystemAPI.Query<RefRO<UnitTypeHolder>>().WithEntityAccess())
        {
            var unitType = unitTypeHolder.ValueRO.unitType;
            if (unitType == UnitTypeSO.UnitType.CursedKing ||
                unitType == UnitTypeSO.UnitType.CursedQueen ||
                unitType == UnitTypeSO.UnitType.CursedBishop ||
                unitType == UnitTypeSO.UnitType.CursedKnight ||
                unitType == UnitTypeSO.UnitType.CursedRook ||
                unitType == UnitTypeSO.UnitType.CursedPawn)
            {
                cursedCount++;
                
                // Entferne FindTarget-Komponente falls vorhanden (verhindert Überschreibung des Ziels)
                if (SystemAPI.HasComponent<FindTarget>(entity))
                {
                    state.EntityManager.RemoveComponent<FindTarget>(entity);
                    Debug.Log($"FindTarget-Komponente von {unitType} entfernt");
                }
                
                // Füge Target-Komponente hinzu
                if (!SystemAPI.HasComponent<Target>(entity))
                {
                    state.EntityManager.AddComponent<Target>(entity);
                }
                
                // Setze das Ziel auf den König
                state.EntityManager.SetComponentData(entity, new Target
                {
                    targetEntity = kingEntity
                });
                
                // Füge MeleeAttack-Komponente hinzu falls nicht vorhanden
                if (!SystemAPI.HasComponent<MeleeAttack>(entity))
                {
                    state.EntityManager.AddComponent<MeleeAttack>(entity);
                    state.EntityManager.SetComponentData(entity, new MeleeAttack
                    {
                        timerMax = 1.0f,
                        damageAmount = 25,
                        colliderSize = 1.5f,
                        timer = 0f,
                        onAttacked = false
                    });
                }
                
                Debug.Log($"Cursed Einheit {unitType} -> Ziel gesetzt auf King Entity({kingEntity.Index}:{kingEntity.Version})");
            }
        }
        
        Debug.Log($"EnemyTargetKingSystem: {cursedCount} Cursed-Einheiten haben King als Ziel erhalten");
        hasRun = true;
    }
}