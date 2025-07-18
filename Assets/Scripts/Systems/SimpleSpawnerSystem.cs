using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
partial struct SimpleSpawnerSystem : ISystem {

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach ((
            RefRW<SimpleSpawner> spawner,
            RefRO<LocalTransform> transform)
            in SystemAPI.Query<
                RefRW<SimpleSpawner>,
                RefRO<LocalTransform>>()) {

            if (spawner.ValueRO.prefabEntity == Entity.Null) continue;

            spawner.ValueRW.timer -= deltaTime;

            if (spawner.ValueRO.timer <= 0f) {
                // Spawne die Entities
                for (int i = 0; i < spawner.ValueRO.spawnAmount; i++) {
                    Entity newEntity = state.EntityManager.Instantiate(spawner.ValueRO.prefabEntity);
                    
                    // Zufällige Position
                    Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)(i + 1 + SystemAPI.Time.ElapsedTime * 1000));
                    float3 randomPos = new float3(
                        random.NextFloat(-spawner.ValueRO.randomOffset.x, spawner.ValueRO.randomOffset.x),
                        random.NextFloat(-spawner.ValueRO.randomOffset.y, spawner.ValueRO.randomOffset.y),
                        random.NextFloat(-spawner.ValueRO.randomOffset.z, spawner.ValueRO.randomOffset.z)
                    );
                    
                    float3 spawnPos = transform.ValueRO.Position + randomPos;
                    state.EntityManager.SetComponentData(newEntity, LocalTransform.FromPosition(spawnPos));
                }

                // Timer zurücksetzen
                spawner.ValueRW.timer = spawner.ValueRO.spawnInterval;
            }
        }
    }
}