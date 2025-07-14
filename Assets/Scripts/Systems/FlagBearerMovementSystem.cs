using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

/// <summary>
/// System für die Bewegung von Flag-Bearer Entities.
/// Bewegt nur Flag-Bearer zu ihren Zielpositionen.
/// </summary>
[UpdateBefore(typeof(FormationSystem))]
partial struct FlagBearerMovementSystem : ISystem
{
    public const float REACHED_TARGET_POSITION_DISTANCE_SQ = 2f;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Keine speziellen Anforderungen
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        // Job für Flag-Bearer Movement
        FlagBearerMovementJob flagBearerMovementJob = new FlagBearerMovementJob
        {
            deltaTime = deltaTime
        };
        state.Dependency = flagBearerMovementJob.ScheduleParallel(state.Dependency);
    }
}

/// <summary>
/// Job für die Bewegung von Flag-Bearer Entities.
/// </summary>
[BurstCompile]
public partial struct FlagBearerMovementJob : IJobEntity
{
    public float deltaTime;

    public void Execute(ref LocalTransform localTransform, ref FlagBearer flagBearer, ref PhysicsVelocity physicsVelocity)
    {
        // Berechne Bewegungsrichtung zum Ziel
        float3 moveDirection = flagBearer.targetPosition - localTransform.Position;
        
        float reachedTargetDistanceSq = FlagBearerMovementSystem.REACHED_TARGET_POSITION_DISTANCE_SQ;
        if (math.lengthsq(moveDirection) <= reachedTargetDistanceSq)
        {
            // Ziel erreicht - stoppe Bewegung
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            flagBearer.isMoving = false;
            return;
        }

        // Flag-Bearer bewegt sich
        flagBearer.isMoving = true;
        moveDirection = math.normalize(moveDirection);

        // Rotation zur Bewegungsrichtung
        localTransform.Rotation = math.slerp(
            localTransform.Rotation,
            quaternion.LookRotation(moveDirection, math.up()),
            deltaTime * flagBearer.rotationSpeed
        );

        // Bewegung setzen
        physicsVelocity.Linear = moveDirection * flagBearer.moveSpeed;
        physicsVelocity.Angular = float3.zero;
    }
}