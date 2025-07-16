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
    public string unitName;
    public Transform ragdollPrefab;
    public float progressMax;
    public Sprite sprite;
    public ResourceAmount[] spawnCostResourceAmountArray;
    public Transform formationPrefab;
    public Transform flagbearerPrefab;

    [Tooltip("Anzahl der Units in der Formation (inklusive Flag Bearer)")]
    public int formationAmount = 5;

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

    public Entity GetFormationPrefabEntity(EntitiesReferences entitiesReferences) {
        switch (unitType) {
            default:
            case UnitType.None: return default;
            // Carrara Golems Chess Units
            case UnitType.CarraraKing: return entitiesReferences.carraraKingFormationPrefabEntity;
            case UnitType.CarraraQueen: return entitiesReferences.carraraQueenFormationPrefabEntity;
            case UnitType.CarraraBishop: return entitiesReferences.carraraBishopFormationPrefabEntity;
            case UnitType.CarraraKnight: return entitiesReferences.carraraKnightFormationPrefabEntity;
            case UnitType.CarraraRook: return entitiesReferences.carraraRookFormationPrefabEntity;
            case UnitType.CarraraPawn: return entitiesReferences.carraraPawnFormationPrefabEntity;
            // Cursed Ones Chess Units
            case UnitType.CursedKing: return entitiesReferences.cursedKingFormationPrefabEntity;
            case UnitType.UncursedKing: return entitiesReferences.uncursedKingFormationPrefabEntity;
            case UnitType.CursedQueen: return entitiesReferences.cursedQueenFormationPrefabEntity;
            case UnitType.CursedBishop: return entitiesReferences.cursedBishopFormationPrefabEntity;
            case UnitType.CursedKnight: return entitiesReferences.cursedKnightFormationPrefabEntity;
            case UnitType.CursedRook: return entitiesReferences.cursedRookFormationPrefabEntity;
            case UnitType.CursedPawn: return entitiesReferences.cursedPawnFormationPrefabEntity;
        }
    }

    public Entity GetFlagbearerPrefabEntity(EntitiesReferences entitiesReferences) {
        switch (unitType) {
            default:
            case UnitType.None: return default;
            // Carrara Golems Chess Units
            case UnitType.CarraraKing: return entitiesReferences.carraraKingFlagbearerPrefabEntity;
            case UnitType.CarraraQueen: return entitiesReferences.carraraQueenFlagbearerPrefabEntity;
            case UnitType.CarraraBishop: return entitiesReferences.carraraBishopFlagbearerPrefabEntity;
            case UnitType.CarraraKnight: return entitiesReferences.carraraKnightFlagbearerPrefabEntity;
            case UnitType.CarraraRook: return entitiesReferences.carraraRookFlagbearerPrefabEntity;
            case UnitType.CarraraPawn: return entitiesReferences.carraraPawnFlagbearerPrefabEntity;
            // Cursed Ones Chess Units
            case UnitType.CursedKing: return entitiesReferences.cursedKingFlagbearerPrefabEntity;
            case UnitType.UncursedKing: return entitiesReferences.uncursedKingFlagbearerPrefabEntity;
            case UnitType.CursedQueen: return entitiesReferences.cursedQueenFlagbearerPrefabEntity;
            case UnitType.CursedBishop: return entitiesReferences.cursedBishopFlagbearerPrefabEntity;
            case UnitType.CursedKnight: return entitiesReferences.cursedKnightFlagbearerPrefabEntity;
            case UnitType.CursedRook: return entitiesReferences.cursedRookFlagbearerPrefabEntity;
            case UnitType.CursedPawn: return entitiesReferences.cursedPawnFlagbearerPrefabEntity;
        }
    }
}