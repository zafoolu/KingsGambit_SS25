using Unity.Entities;
using UnityEngine;

[CreateAssetMenu()]
public class UnitTypeSO : ScriptableObject {

    public enum UnitType {
        None,
        // Carrara Golems Chess Units
        CarraraKing,
        CarraraQueen,
        CarraraBishop,
        CarraraKnight,
        CarraraRook,
        CarraraPawn,
        // Cursed Ones Chess Units
        CursedKing,
        UncursedKing,
        CursedQueen,
        CursedBishop,
        CursedKnight,
        CursedRook,
        CursedPawn
    }

    public UnitType unitType;
    public Transform ragdollPrefab;
    public float progressMax;
    public Sprite sprite;
    public ResourceAmount[] spawnCostResourceAmountArray;

    public Entity GetPrefabEntity(EntitiesReferences entitiesReferences) {
        switch (unitType) {
            default:
            case UnitType.None: return default;
            // Carrara Golems Chess Units
            case UnitType.CarraraKing: return entitiesReferences.carraraKingPrefabEntity;
            case UnitType.CarraraQueen: return entitiesReferences.carraraQueenPrefabEntity;
            case UnitType.CarraraBishop: return entitiesReferences.carraraBishopPrefabEntity;
            case UnitType.CarraraKnight: return entitiesReferences.carraraKnightPrefabEntity;
            case UnitType.CarraraRook: return entitiesReferences.carraraRookPrefabEntity;
            case UnitType.CarraraPawn: return entitiesReferences.carraraPawnPrefabEntity;
            // Cursed Ones Chess Units
            case UnitType.CursedKing: return entitiesReferences.cursedKingPrefabEntity;
            case UnitType.UncursedKing: return entitiesReferences.uncursedKingPrefabEntity;
            case UnitType.CursedQueen: return entitiesReferences.cursedQueenPrefabEntity;
            case UnitType.CursedBishop: return entitiesReferences.cursedBishopPrefabEntity;
            case UnitType.CursedKnight: return entitiesReferences.cursedKnightPrefabEntity;
            case UnitType.CursedRook: return entitiesReferences.cursedRookPrefabEntity;
            case UnitType.CursedPawn: return entitiesReferences.cursedPawnPrefabEntity;
        }
    }
}