using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateAfter(typeof(UnitMoverSystem))]
public partial struct UnitCollisionAvoidanceSystem : ISystem {

    private const float AVOIDANCE_RADIUS = 2f; // Radius um die Einheit für Kollisionserkennung
    private const float AVOIDANCE_FORCE = 5f; // Stärke der Ausweichbewegung

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<UnitMover>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        float deltaTime = SystemAPI.Time.DeltaTime;
        
        // Erstelle einen Job für die Kollisionsvermeidung
        UnitCollisionAvoidanceJob collisionAvoidanceJob = new UnitCollisionAvoidanceJob {
            deltaTime = deltaTime,
        };

        collisionAvoidanceJob.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct UnitCollisionAvoidanceJob : IJobEntity {

    public float deltaTime;

    void Execute(ref LocalTransform transform, ref UnitMover unitMover, in PhysicsCollider collider) {
        if (!unitMover.isMoving) return; // Nur bewegende Einheiten berücksichtigen

        float3 avoidanceForce = float3.zero;
        float3 currentPosition = transform.Position;

        // Überprüfe Kollisionen mit anderen Einheiten in der Nähe
        var overlaps = PhysicsWorld.OverlapSphere(currentPosition, AVOIDANCE_RADIUS, new CollisionFilter {
            BelongsTo = ~0u,
            CollidesWith = 1u << GameAssets.UNIT_LAYER, // Nur Kollisionen mit anderen Einheiten
            GroupIndex = 0
        });

        // Berechne Ausweichkraft basierend auf nahen Einheiten
        foreach (var overlap in overlaps) {
            if (overlap.Entity == Entity.Null) continue;

            float3 otherPosition = overlap.Transform.Position;
            float3 direction = currentPosition - otherPosition;
            float distance = math.length(direction);

            if (distance < 0.0001f) continue; // Vermeide Division durch Null

            // Je näher die andere Einheit, desto stärker die Ausweichkraft
            float forceMagnitude = AVOIDANCE_FORCE * (1.0f - distance / AVOIDANCE_RADIUS);
            avoidanceForce += math.normalize(direction) * forceMagnitude;
        }

        // Modifiziere die Zielposition basierend auf der Ausweichkraft
        unitMover.targetPosition += avoidanceForce * deltaTime;
    }
}