using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class BuildingDestructionManager : MonoBehaviour {


    [SerializeField] private Transform buildingHQVisualBrokenPartsPrefab;
    [SerializeField] private Transform buildingBarracksVisualBrokenPartsPrefab;
    [SerializeField] private Transform buildingTowerBrokenPartsPrefab;
    [SerializeField] private Transform buildingGoldHarvesterBrokenPartsPrefab;
    [SerializeField] private Transform buildingIronHarvesterBrokenPartsPrefab;
    [SerializeField] private Transform buildingOilHarvesterBrokenPartsPrefab;



    private void Start() {
        DOTSEventsManager.Instance.OnHealthDead += DOTSEventsManager_OnHealthDead;
    }

    private void DOTSEventsManager_OnHealthDead(object sender, System.EventArgs e) {
        Entity entity = (Entity)sender;

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (entityManager.HasComponent<BuildingTypeSOHolder>(entity)) {
            // Building Destroyed - Spawn broken parts visual effects
            BuildingTypeSOHolder buildingTypeSOHolder = entityManager.GetComponentData<BuildingTypeSOHolder>(entity);
            LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(entity);

            // Note: Broken parts spawning logic can be implemented here when needed
            // Currently using the new EnemySpawner system instead of old building-based spawning
        }
    }

}


