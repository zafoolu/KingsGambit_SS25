using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

/// <summary>
/// System für die Bewegung von Formation-Follower Units.
/// Bewegt Units zu ihren berechneten Formation-Positionen.
/// </summary>
[UpdateAfter(typeof(FormationSystem))]
partial struct FormationFollowerMovementSystem : ISystem
{
    public const float REACHED_TARGET_POSITION_DISTANCE_SQ = 0.25f; // 0.5 Units Radius

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Keine speziellen Anforderungen
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        // Job für Formation-Follower Movement
        FormationFollowerMovementJob movementJob = new FormationFollowerMovementJob
        {
            deltaTime = deltaTime
        };
        state.Dependency = movementJob.ScheduleParallel(state.Dependency);
    }
}

/// <summary>
/// Job für die Bewegung von Formation-Follower Units.
/// </summary>
[BurstCompile]
public partial struct FormationFollowerMovementJob : IJobEntity
{
    public float deltaTime;

    public void Execute(ref LocalTransform localTransform, ref FormationFollower formationFollower, ref PhysicsVelocity physicsVelocity)
    {
        // Prüfe ob gültige Zielposition vorhanden
        if (math.all(formationFollower.targetPosition == float3.zero))
        {
            // Keine Zielposition - stoppe Bewegung
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            formationFollower.isMoving = false;
            return;
        }

        // Berechne Bewegungsrichtung zum Ziel
        float3 moveDirection = formationFollower.targetPosition - localTransform.Position;
        
        float reachedTargetDistanceSq = FormationFollowerMovementSystem.REACHED_TARGET_POSITION_DISTANCE_SQ;
        if (math.lengthsq(moveDirection) <= reachedTargetDistanceSq)
        {
            // Ziel erreicht - stoppe Bewegung
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            formationFollower.isMoving = false;
            return;
        }

        // Unit bewegt sich
        formationFollower.isMoving = true;
        
        float distanceToTarget = math.length(moveDirection);
        moveDirection = math.normalize(moveDirection);

        // Rotation zur Bewegungsrichtung
        localTransform.Rotation = math.slerp(
            localTransform.Rotation,
            quaternion.LookRotation(moveDirection, math.up()),
            deltaTime * formationFollower.rotationSpeed
        );

        // Geschwindigkeit reduzieren wenn nahe am Ziel (sanftes Bremsen)
        float speedMultiplier = math.min(1f, distanceToTarget / 2f); // Bremsen ab 2 Units Entfernung
        speedMultiplier = math.max(0.1f, speedMultiplier); // Mindestgeschwindigkeit
        
        // Bewegung setzen
        physicsVelocity.Linear = moveDirection * formationFollower.moveSpeed * speedMultiplier;
        physicsVelocity.Angular = float3.zero;
    }
}