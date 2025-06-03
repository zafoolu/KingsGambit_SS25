using Unity.Entities;
using UnityEngine;

public struct EntitiesReferences : IComponentData {
    public Entity bulletPrefabEntity;
    public Entity buildingConstructionPrefabEntity;

    // Carrara Golems Buildings
    public Entity carraraGolemWorkshopPrefabEntity;
    public Entity carraraGolemTownhallPrefabEntity;
    public Entity carraraGolemSmelteryPrefabEntity;
    public Entity carraraGolemQuarryPrefabEntity;
    public Entity carraraGolemBarracksPrefabEntity;

    // Carrara Golems Building Visuals
    public Entity carraraGolemWorkshopVisualPrefabEntity;
    public Entity carraraGolemTownhallVisualPrefabEntity;
    public Entity carraraGolemSmelteryVisualPrefabEntity;
    public Entity carraraGolemQuarryVisualPrefabEntity;
    public Entity carraraGolemBarracksVisualPrefabEntity;

    // Cursed Ones Buildings
    public Entity cursedOnesTownhallPrefabEntity;
    public Entity cursedOnesRuinsPrefabEntity;
    public Entity cursedOnesGoldminePrefabEntity;
    public Entity cursedOnesCathedralPrefabEntity;
    public Entity cursedOnesBloodCirclePrefabEntity;
    public Entity cursedOnesAltarPrefabEntity;

    // Cursed Ones Building Visuals
    public Entity cursedOnesTownhallVisualPrefabEntity;
    public Entity cursedOnesRuinsVisualPrefabEntity;
    public Entity cursedOnesGoldmineVisualPrefabEntity;
    public Entity cursedOnesCathedralVisualPrefabEntity;
    public Entity cursedOnesBloodCircleVisualPrefabEntity;
    public Entity cursedOnesAltarVisualPrefabEntity;

    // Carrara Golems Chess Units
    public Entity carraraKingPrefabEntity;
    public Entity carraraQueenPrefabEntity;
    public Entity carraraBishopPrefabEntity;
    public Entity carraraKnightPrefabEntity;
    public Entity carraraRookPrefabEntity;
    public Entity carraraPawnPrefabEntity;

    // Cursed Ones Chess Units
    public Entity cursedKingPrefabEntity;
    public Entity uncursedKingPrefabEntity;
    public Entity cursedQueenPrefabEntity;
    public Entity cursedBishopPrefabEntity;
    public Entity cursedKnightPrefabEntity;
    public Entity cursedRookPrefabEntity;
    public Entity cursedPawnPrefabEntity;
}

public class EntitiesReferencesAuthoring : MonoBehaviour {
    public GameObject bulletPrefabGameObject;
    public GameObject buildingConstructionPrefabGameObject;

    // Carrara Golems Buildings
    public GameObject carraraGolemWorkshopPrefabGameObject;
    public GameObject carraraGolemTownhallPrefabGameObject;
    public GameObject carraraGolemSmelteryPrefabGameObject;
    public GameObject carraraGolemQuarryPrefabGameObject;
    public GameObject carraraGolemBarracksPrefabGameObject;

    // Carrara Golems Building Visuals
    public GameObject carraraGolemWorkshopVisualPrefabGameObject;
    public GameObject carraraGolemTownhallVisualPrefabGameObject;
    public GameObject carraraGolemSmelteryVisualPrefabGameObject;
    public GameObject carraraGolemQuarryVisualPrefabGameObject;
    public GameObject carraraGolemBarracksVisualPrefabGameObject;

    // Cursed Ones Buildings
    public GameObject cursedOnesTownhallPrefabGameObject;
    public GameObject cursedOnesRuinsPrefabGameObject;
    public GameObject cursedOnesGoldminePrefabGameObject;
    public GameObject cursedOnesCathedralPrefabGameObject;
    public GameObject cursedOnesBloodCirclePrefabGameObject;
    public GameObject cursedOnesAltarPrefabGameObject;

    // Cursed Ones Building Visuals
    public GameObject cursedOnesTownhallVisualPrefabGameObject;
    public GameObject cursedOnesRuinsVisualPrefabGameObject;
    public GameObject cursedOnesGoldmineVisualPrefabGameObject;
    public GameObject cursedOnesCathedralVisualPrefabGameObject;
    public GameObject cursedOnesBloodCircleVisualPrefabGameObject;
    public GameObject cursedOnesAltarVisualPrefabGameObject;

    // Carrara Golems Chess Units
    public GameObject carraraKingPrefabGameObject;
    public GameObject carraraQueenPrefabGameObject;
    public GameObject carraraBishopPrefabGameObject;
    public GameObject carraraKnightPrefabGameObject;
    public GameObject carraraRookPrefabGameObject;
    public GameObject carraraPawnPrefabGameObject;

    // Cursed Ones Chess Units
    public GameObject cursedKingPrefabGameObject;
    public GameObject uncursedKingPrefabGameObject;
    public GameObject cursedQueenPrefabGameObject;
    public GameObject cursedBishopPrefabGameObject;
    public GameObject cursedKnightPrefabGameObject;
    public GameObject cursedRookPrefabGameObject;
    public GameObject cursedPawnPrefabGameObject;

    public class Baker : Baker<EntitiesReferencesAuthoring> {
        public override void Bake(EntitiesReferencesAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences {
                bulletPrefabEntity = GetEntity(authoring.bulletPrefabGameObject, TransformUsageFlags.Dynamic),
                buildingConstructionPrefabEntity = GetEntity(authoring.buildingConstructionPrefabGameObject, TransformUsageFlags.Dynamic),

                // Carrara Golems Buildings
                carraraGolemWorkshopPrefabEntity = GetEntity(authoring.carraraGolemWorkshopPrefabGameObject, TransformUsageFlags.Dynamic),
                carraraGolemTownhallPrefabEntity = GetEntity(authoring.carraraGolemTownhallPrefabGameObject, TransformUsageFlags.Dynamic),
                carraraGolemSmelteryPrefabEntity = GetEntity(authoring.carraraGolemSmelteryPrefabGameObject, TransformUsageFlags.Dynamic),
                carraraGolemQuarryPrefabEntity = GetEntity(authoring.carraraGolemQuarryPrefabGameObject, TransformUsageFlags.Dynamic),
                carraraGolemBarracksPrefabEntity = GetEntity(authoring.carraraGolemBarracksPrefabGameObject, TransformUsageFlags.Dynamic),

                // Carrara Golems Building Visuals
                carraraGolemWorkshopVisualPrefabEntity = GetEntity(authoring.carraraGolemWorkshopVisualPrefabGameObject, TransformUsageFlags.Dynamic),
                carraraGolemTownhallVisualPrefabEntity = GetEntity(authoring.carraraGolemTownhallVisualPrefabGameObject, TransformUsageFlags.Dynamic),
                carraraGolemSmelteryVisualPrefabEntity = GetEntity(authoring.carraraGolemSmelteryVisualPrefabGameObject, TransformUsageFlags.Dynamic),
                carraraGolemQuarryVisualPrefabEntity = GetEntity(authoring.carraraGolemQuarryVisualPrefabGameObject, TransformUsageFlags.Dynamic),
                carraraGolemBarracksVisualPrefabEntity = GetEntity(authoring.carraraGolemBarracksVisualPrefabGameObject, TransformUsageFlags.Dynamic),

                // Cursed Ones Buildings
                cursedOnesTownhallPrefabEntity = GetEntity(authoring.cursedOnesTownhallPrefabGameObject, TransformUsageFlags.Dynamic),
                cursedOnesRuinsPrefabEntity = GetEntity(authoring.cursedOnesRuinsPrefabGameObject, TransformUsageFlags.Dynamic),
                cursedOnesGoldminePrefabEntity = GetEntity(authoring.cursedOnesGoldminePrefabGameObject, TransformUsageFlags.Dynamic),
                cursedOnesCathedralPrefabEntity = GetEntity(authoring.cursedOnesCathedralPrefabGameObject, TransformUsageFlags.Dynamic),
                cursedOnesBloodCirclePrefabEntity = GetEntity(authoring.cursedOnesBloodCirclePrefabGameObject, TransformUsageFlags.Dynamic),
                cursedOnesAltarPrefabEntity = GetEntity(authoring.cursedOnesAltarPrefabGameObject, TransformUsageFlags.Dynamic),

                // Cursed Ones Building Visuals
                cursedOnesTownhallVisualPrefabEntity = GetEntity(authoring.cursedOnesTownhallVisualPrefabGameObject, TransformUsageFlags.Dynamic),
                cursedOnesRuinsVisualPrefabEntity = GetEntity(authoring.cursedOnesRuinsVisualPrefabGameObject, TransformUsageFlags.Dynamic),
                cursedOnesGoldmineVisualPrefabEntity = GetEntity(authoring.cursedOnesGoldmineVisualPrefabGameObject, TransformUsageFlags.Dynamic),
                cursedOnesCathedralVisualPrefabEntity = GetEntity(authoring.cursedOnesCathedralVisualPrefabGameObject, TransformUsageFlags.Dynamic),
                cursedOnesBloodCircleVisualPrefabEntity = GetEntity(authoring.cursedOnesBloodCircleVisualPrefabGameObject, TransformUsageFlags.Dynamic),
                cursedOnesAltarVisualPrefabEntity = GetEntity(authoring.cursedOnesAltarVisualPrefabGameObject, TransformUsageFlags.Dynamic),

                // Carrara Golems Chess Units
                carraraKingPrefabEntity = GetEntity(authoring.carraraKingPrefabGameObject, TransformUsageFlags.Dynamic),
                carraraQueenPrefabEntity = GetEntity(authoring.carraraQueenPrefabGameObject, TransformUsageFlags.Dynamic),
                carraraBishopPrefabEntity = GetEntity(authoring.carraraBishopPrefabGameObject, TransformUsageFlags.Dynamic),
                carraraKnightPrefabEntity = GetEntity(authoring.carraraKnightPrefabGameObject, TransformUsageFlags.Dynamic),
                carraraRookPrefabEntity = GetEntity(authoring.carraraRookPrefabGameObject, TransformUsageFlags.Dynamic),
                carraraPawnPrefabEntity = GetEntity(authoring.carraraPawnPrefabGameObject, TransformUsageFlags.Dynamic),

                // Cursed Ones Chess Units
                cursedKingPrefabEntity = GetEntity(authoring.cursedKingPrefabGameObject, TransformUsageFlags.Dynamic),
                uncursedKingPrefabEntity = GetEntity(authoring.uncursedKingPrefabGameObject, TransformUsageFlags.Dynamic),
                cursedQueenPrefabEntity = GetEntity(authoring.cursedQueenPrefabGameObject, TransformUsageFlags.Dynamic),
                cursedBishopPrefabEntity = GetEntity(authoring.cursedBishopPrefabGameObject, TransformUsageFlags.Dynamic),
                cursedKnightPrefabEntity = GetEntity(authoring.cursedKnightPrefabGameObject, TransformUsageFlags.Dynamic),
                cursedRookPrefabEntity = GetEntity(authoring.cursedRookPrefabGameObject, TransformUsageFlags.Dynamic),
                cursedPawnPrefabEntity = GetEntity(authoring.cursedPawnPrefabGameObject, TransformUsageFlags.Dynamic),
            });
        }
    }
}