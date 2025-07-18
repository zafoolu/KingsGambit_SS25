using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SimpleSpawnerAuthoring : MonoBehaviour {

    [Header("Was spawnen?")]
    public Transform prefabToSpawn;
    
    [Header("Wie oft?")]
    public float spawnInterval = 2f;
    public int spawnAmount = 1;
    
    [Header("Wo spawnen?")]
    public float3 randomOffset = new float3(2, 0, 2);

    public class Baker : Baker<SimpleSpawnerAuthoring> {
        public override void Bake(SimpleSpawnerAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            Entity prefabEntity = Entity.Null;
            if (authoring.prefabToSpawn != null) {
                prefabEntity = GetEntity(authoring.prefabToSpawn, TransformUsageFlags.Dynamic);
            }

            AddComponent(entity, new SimpleSpawner {
                prefabEntity = prefabEntity,
                spawnInterval = authoring.spawnInterval,
                spawnAmount = authoring.spawnAmount,
                randomOffset = authoring.randomOffset,
                timer = authoring.spawnInterval
            });
        }
    }
}

public struct SimpleSpawner : IComponentData {
    public Entity prefabEntity;
    public float spawnInterval;
    public int spawnAmount;
    public float3 randomOffset;
    public float timer;
}