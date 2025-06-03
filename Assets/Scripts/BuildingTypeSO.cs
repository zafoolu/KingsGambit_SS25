using Unity.Entities;
using UnityEngine;

[CreateAssetMenu()]
public class BuildingTypeSO : ScriptableObject {


    public enum BuildingType {
        None,
        
        // Carrara Golems buildings
        CarraraWorkshop,
        CarraraTownhall,
        CarraraSmeltery,
        CarraraQuarry,
        CarraraBarracks,
        
        // Cursed Ones buildings
        CursedTownhall,
        CursedRuins,
        CursedGoldmine,
        CursedCathedral,
        CursedBloodcircle,
        CursedAltar,
    }


    public string nameString;
    public BuildingType buildingType;
    public float buildingConstructionTimerMax;
    public float constructionYOffset;
    public Transform prefab;
    public float buildingDistanceMin;
    public bool showInBuildingPlacementManagerUI;
    public Sprite sprite;
    public Transform visualPrefab;
    public ResourceAmount[] buildCostResourceAmountArray;


    public bool IsNone() {
        return buildingType == BuildingType.None;
    }

    public Entity GetPrefabEntity(EntitiesReferences entitiesReferences) {
        switch (buildingType) {
            case BuildingType.None:
                return Entity.Null;
            
            // Carrara Golems buildings
            case BuildingType.CarraraWorkshop:
                return entitiesReferences.carraraGolemWorkshopPrefabEntity;
            case BuildingType.CarraraTownhall:
                return entitiesReferences.carraraGolemTownhallPrefabEntity;
            case BuildingType.CarraraSmeltery:
                return entitiesReferences.carraraGolemSmelteryPrefabEntity;
            case BuildingType.CarraraQuarry:
                return entitiesReferences.carraraGolemQuarryPrefabEntity;
            case BuildingType.CarraraBarracks:
                return entitiesReferences.carraraGolemBarracksPrefabEntity;
            
            // Cursed Ones buildings
            case BuildingType.CursedTownhall:
                return entitiesReferences.cursedOnesTownhallPrefabEntity;
            case BuildingType.CursedRuins:
                return entitiesReferences.cursedOnesRuinsPrefabEntity;
            case BuildingType.CursedGoldmine:
                return entitiesReferences.cursedOnesGoldminePrefabEntity;
            case BuildingType.CursedCathedral:
                return entitiesReferences.cursedOnesCathedralPrefabEntity;
            case BuildingType.CursedBloodcircle:
                return entitiesReferences.cursedOnesBloodCirclePrefabEntity;
            case BuildingType.CursedAltar:
                return entitiesReferences.cursedOnesAltarPrefabEntity;
            default:
                return Entity.Null;
        }
    }

    public Entity GetVisualPrefabEntity(EntitiesReferences entitiesReferences) {
        switch (buildingType) {
            case BuildingType.None:
                return Entity.Null;
            
            // Carrara Golems buildings
            case BuildingType.CarraraWorkshop:
                return entitiesReferences.carraraGolemWorkshopVisualPrefabEntity;
            case BuildingType.CarraraTownhall:
                return entitiesReferences.carraraGolemTownhallVisualPrefabEntity;
            case BuildingType.CarraraSmeltery:
                return entitiesReferences.carraraGolemSmelteryVisualPrefabEntity;
            case BuildingType.CarraraQuarry:
                return entitiesReferences.carraraGolemQuarryVisualPrefabEntity;
            case BuildingType.CarraraBarracks:
                return entitiesReferences.carraraGolemBarracksVisualPrefabEntity;
            
            // Cursed Ones buildings
            case BuildingType.CursedTownhall:
                return entitiesReferences.cursedOnesTownhallVisualPrefabEntity;
            case BuildingType.CursedRuins:
                return entitiesReferences.cursedOnesRuinsVisualPrefabEntity;
            case BuildingType.CursedGoldmine:
                return entitiesReferences.cursedOnesGoldmineVisualPrefabEntity;
            case BuildingType.CursedCathedral:
                return entitiesReferences.cursedOnesCathedralVisualPrefabEntity;
            case BuildingType.CursedBloodcircle:
                return entitiesReferences.cursedOnesBloodCircleVisualPrefabEntity;
            case BuildingType.CursedAltar:
                return entitiesReferences.cursedOnesAltarVisualPrefabEntity;
            default:
                return Entity.Null;
        }
    }

}