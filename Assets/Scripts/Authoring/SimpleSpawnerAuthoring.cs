using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SimpleSpawnerAuthoring : MonoBehaviour {
    public GameObject prefab;
    public float spawnInterval = 2f;
    public int spawnAmount = 1;
    public float3 randomOffset = new float3(1f, 0f, 1f);
    public float moveSpeed = 5f;
    public Transform targetPoint;

    public class Baker : Baker<SimpleSpawnerAuthoring> {
        public override void Bake(SimpleSpawnerAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            // Bestimme die Zielposition
            float3 targetPosition = float3.zero;
            if (authoring.targetPoint != null) {
                targetPosition = authoring.targetPoint.position;
                #if UNITY_EDITOR
                Debug.Log($"Baker: Target Point set to {targetPosition.x}, {targetPosition.y}, {targetPosition.z}");
                #endif
            } else {
                #if UNITY_EDITOR
                Debug.LogWarning("Baker: No target point assigned! Entities will use spawn position as target.");
                #endif
            }
            
            AddComponent(entity, new SimpleSpawner {
                prefabEntity = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                spawnInterval = authoring.spawnInterval,
                timer = authoring.spawnInterval,
                spawnAmount = authoring.spawnAmount,
                randomOffset = authoring.randomOffset,
                moveSpeed = authoring.moveSpeed,
                targetPosition = targetPosition
            });
        }
    }
}

public struct SimpleSpawner : IComponentData {
    public Entity prefabEntity;
    public float spawnInterval;
    public float timer;
    public int spawnAmount;
    public float3 randomOffset;
    public float moveSpeed;
    public float3 targetPosition;
}

public struct MoveToTarget : IComponentData {
    public float3 targetPosition;
    public float speed;
}