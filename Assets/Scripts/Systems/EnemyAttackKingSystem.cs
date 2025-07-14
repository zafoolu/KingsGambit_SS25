using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct EnemyAttackKingSystem : ISystem {


    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<King>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        Entity kingEntity = SystemAPI.GetSingletonEntity<King>();
        float3 kingPosition = SystemAPI.GetComponent<LocalTransform>(kingEntity).Position;

        foreach ((
            RefRO<EnemyAttackHQ> enemyAttackHQ,
            RefRW<TargetPositionPathQueued> targetPositionPathQueued,
            EnabledRefRW<TargetPositionPathQueued> targetPositionPathQueuedEnabled,
            RefRO<Target> target)
            in SystemAPI.Query<
                RefRO<EnemyAttackHQ>,
                RefRW<TargetPositionPathQueued>,
                EnabledRefRW<TargetPositionPathQueued>,
                RefRO<Target>>().WithDisabled<MoveOverride>().WithPresent<TargetPositionPathQueued>()) {

            if (target.ValueRO.targetEntity != Entity.Null) {
                continue;
            }

            targetPositionPathQueued.ValueRW.targetPosition = kingPosition;
            targetPositionPathQueuedEnabled.ValueRW = true;
        }

    }


}