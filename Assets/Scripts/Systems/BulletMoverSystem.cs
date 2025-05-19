using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct BulletMoverSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        float deltaTime = SystemAPI.Time.DeltaTime;
        const float moveSpeed = 10f;
        const float destroyDistanceSq = 0.1f * 0.1f;

        foreach ((RefRW<LocalTransform> localTransform,
            RefRW<Bullet> bullet,
            RefRO<Target> target,
            Entity bulletEntity) in SystemAPI.Query<
                RefRW<LocalTransform>,
                RefRW<Bullet>,
                RefRO<Target>>()
                .WithEntityAccess())
        {
            float3 targetPosition;
            Entity targetEntity = target.ValueRO.targetEntity;

            if (targetEntity == Entity.Null || !SystemAPI.Exists(targetEntity))
            {
                targetPosition = bullet.ValueRO.lastTargetPosition;
            }
            else
            {
                if (!SystemAPI.HasComponent<LocalTransform>(targetEntity) ||
                    !SystemAPI.HasComponent<ShootVictim>(targetEntity))
                {
                    targetPosition = bullet.ValueRO.lastTargetPosition;
                }
                else
                {
                    var targetTransform = SystemAPI.GetComponent<LocalTransform>(targetEntity);
                    var shootVictim = SystemAPI.GetComponent<ShootVictim>(targetEntity);
                    targetPosition = targetTransform.TransformPoint(shootVictim.hitLocalPosition);
                }
            }

            float3 moveDir = math.normalize(targetPosition - localTransform.ValueRO.Position);
            localTransform.ValueRW.Position += moveDir * deltaTime * moveSpeed;

            if (math.distancesq(localTransform.ValueRO.Position, targetPosition) < destroyDistanceSq)
            {
                if (targetEntity != Entity.Null &&
                    SystemAPI.Exists(targetEntity) &&
                    SystemAPI.HasComponent<Health>(targetEntity))
                {
                    var targetHealth = SystemAPI.GetComponentRW<Health>(targetEntity);
                    targetHealth.ValueRW.healthAmount -= bullet.ValueRO.damageAmount;
                    targetHealth.ValueRW.onHealthChanged = true;
                    targetHealth.ValueRW.onTookDamage = true;
                }

                entityCommandBuffer.DestroyEntity(bulletEntity);
            }
        }
    }
}