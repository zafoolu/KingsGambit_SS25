using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

partial struct UnitMoverSystem : ISystem {

    public const float REACHED_TARGET_POSITION_DISTANCE_SQ = 2f;

    public ComponentLookup<TargetPositionPathQueued> targetPositionPathQueuedComponentLookup;
    public ComponentLookup<FlowFieldPathRequest> flowFieldPathRequestComponentLookup;
    public ComponentLookup<FlowFieldFollower> flowFieldFollowerComponentLookup;
    public ComponentLookup<MoveOverride> moveOverrideComponentLookup;
    public ComponentLookup<GridSystem.GridNode> gridNodeComponentLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<GridSystem.GridSystemData>();

        targetPositionPathQueuedComponentLookup = SystemAPI.GetComponentLookup<TargetPositionPathQueued>(false);
        flowFieldPathRequestComponentLookup = SystemAPI.GetComponentLookup<FlowFieldPathRequest>(false);
        flowFieldFollowerComponentLookup = SystemAPI.GetComponentLookup<FlowFieldFollower>(false);
        moveOverrideComponentLookup = SystemAPI.GetComponentLookup<MoveOverride>(false);
        gridNodeComponentLookup = SystemAPI.GetComponentLookup<GridSystem.GridNode>(false);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        GridSystem.GridSystemData gridSystemData = SystemAPI.GetSingleton<GridSystem.GridSystemData>();

        targetPositionPathQueuedComponentLookup.Update(ref state);
        flowFieldPathRequestComponentLookup.Update(ref state);
        flowFieldFollowerComponentLookup.Update(ref state);
        moveOverrideComponentLookup.Update(ref state);
        gridNodeComponentLookup.Update(ref state);

        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        TargetPositionPathQueuedJob targetPositionPathQueuedJob = new TargetPositionPathQueuedJob {
            collisionWorld = collisionWorld,
            gridNodeSize = gridSystemData.gridNodeSize,
            width = gridSystemData.width,
            height = gridSystemData.height,
            costMap = gridSystemData.costMap,
            flowFieldFollowerComponentLookup = flowFieldFollowerComponentLookup,
            flowFieldPathRequestComponentLookup = flowFieldPathRequestComponentLookup,
            moveOverrideComponentLookup = moveOverrideComponentLookup,
            targetPositionPathQueuedComponentLookup = targetPositionPathQueuedComponentLookup
        };
        targetPositionPathQueuedJob.ScheduleParallel();

        TestCanMoveStraightJob testCanMoveStraightJob = new TestCanMoveStraightJob {
            collisionWorld = collisionWorld,
            flowFieldFollowerComponentLookup = flowFieldFollowerComponentLookup,
        };
        testCanMoveStraightJob.ScheduleParallel();

        FlowFieldFollowerJob flowFieldFollowerJob = new FlowFieldFollowerJob {
            width = gridSystemData.width,
            height = gridSystemData.height,
            gridNodeSize = gridSystemData.gridNodeSize,
            gridNodeSizeDouble = gridSystemData.gridNodeSize * 2f,
            flowFieldFollowerComponentLookup = flowFieldFollowerComponentLookup,
            totalGridMapEntityArray = gridSystemData.totalGridMapEntityArray,
            gridNodeComponentLookup = gridNodeComponentLookup,
        };
        flowFieldFollowerJob.ScheduleParallel();

        UnitMoverJob unitMoverJob = new UnitMoverJob {
            deltaTime = SystemAPI.Time.DeltaTime,
            physicsWorld = physicsWorldSingleton.PhysicsWorld
        };
        unitMoverJob.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct UnitMoverJob : IJobEntity {

    public float deltaTime;
    [ReadOnly] public PhysicsWorld physicsWorld;

    public void Execute(ref LocalTransform localTransform, ref UnitMover unitMover, ref PhysicsVelocity physicsVelocity) {
        float3 moveDirection = unitMover.targetPosition - localTransform.Position;

        float reachedTargetDistanceSq = UnitMoverSystem.REACHED_TARGET_POSITION_DISTANCE_SQ;
        if (math.lengthsq(moveDirection) <= reachedTargetDistanceSq) {
            // Reached the target position
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            unitMover.isMoving = false;
            return;
        }

        unitMover.isMoving = true;
        moveDirection = math.normalize(moveDirection);

        // Überprüfe auf nahe Einheiten und berechne Ausweichrichtung
        float3 avoidanceDirection = float3.zero;
        NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);
        
        // Suche nach Einheiten in der Nähe
        if (physicsWorld.OverlapSphere(localTransform.Position, unitMover.personalSpace, ref hits, 
            new CollisionFilter { BelongsTo = ~0u, CollidesWith = 1u << GameAssets.UNIT_LAYER, GroupIndex = 0 })) {
            
            for (int i = 0; i < hits.Length; i++) {
                DistanceHit hit = hits[i];
                float3 directionToOther = localTransform.Position - hit.Position;
                float distance = math.length(directionToOther);
                
                if (distance > 0) {
                    // Je näher die andere Einheit, desto stärker die Ausweichkraft
                    float weight = 1.0f - (distance / unitMover.personalSpace);
                    avoidanceDirection += math.normalize(directionToOther) * weight;
                }
            }
        }
        hits.Dispose();

        // Kombiniere Bewegungs- und Ausweichrichtung
        if (math.lengthsq(avoidanceDirection) > 0) {
            avoidanceDirection = math.normalize(avoidanceDirection);
            moveDirection = math.lerp(moveDirection, avoidanceDirection, unitMover.avoidanceWeight);
            moveDirection = math.normalize(moveDirection);
        }

        // Aktualisiere Rotation und Bewegung
        localTransform.Rotation = math.slerp(
            localTransform.Rotation,
            quaternion.LookRotation(moveDirection, math.up()),
            deltaTime * unitMover.rotationSpeed
        );

        physicsVelocity.Linear = moveDirection * unitMover.moveSpeed;
        physicsVelocity.Angular = float3.zero;
    }
}

[BurstCompile]
[WithAll(typeof(TargetPositionPathQueued))]
public partial struct TargetPositionPathQueuedJob : IJobEntity {

    [NativeDisableParallelForRestriction] public ComponentLookup<TargetPositionPathQueued> targetPositionPathQueuedComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldPathRequest> flowFieldPathRequestComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldFollower> flowFieldFollowerComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<MoveOverride> moveOverrideComponentLookup;

    [ReadOnly] public CollisionWorld collisionWorld;
    [ReadOnly] public int width;
    [ReadOnly] public int height;
    [ReadOnly] public NativeArray<int> costMap;
    [ReadOnly] public float gridNodeSize;

    public void Execute(
        in LocalTransform localTransform,
        ref UnitMover unitMover,
        Entity entity) {

        RaycastInput raycastInput = new RaycastInput {
            Start = localTransform.Position,
            End = targetPositionPathQueuedComponentLookup[entity].targetPosition,
            Filter = new CollisionFilter {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.PATHFINDING_WALLS,
                GroupIndex = 0
            }
        };

        if (!collisionWorld.CastRay(raycastInput)) {
            // Did not hit anything, no walls in between
            unitMover.targetPosition = targetPositionPathQueuedComponentLookup[entity].targetPosition;
            flowFieldPathRequestComponentLookup.SetComponentEnabled(entity, false);
            flowFieldFollowerComponentLookup.SetComponentEnabled(entity, false);
        } else {
            // There's a wall in between
            if (moveOverrideComponentLookup.HasComponent(entity)) {
                moveOverrideComponentLookup.SetComponentEnabled(entity, false);
            }
            if (GridSystem.IsValidWalkableGridPosition(targetPositionPathQueuedComponentLookup[entity].targetPosition, width, height, costMap, gridNodeSize)) {
                FlowFieldPathRequest flowFieldPathRequest = flowFieldPathRequestComponentLookup[entity];
                flowFieldPathRequest.targetPosition = targetPositionPathQueuedComponentLookup[entity].targetPosition;
                flowFieldPathRequestComponentLookup[entity] = flowFieldPathRequest;
                flowFieldPathRequestComponentLookup.SetComponentEnabled(entity, true);
            } else {
                unitMover.targetPosition = localTransform.Position;
                flowFieldPathRequestComponentLookup.SetComponentEnabled(entity, false);
                flowFieldFollowerComponentLookup.SetComponentEnabled(entity, false);
            }
        }

        targetPositionPathQueuedComponentLookup.SetComponentEnabled(entity, false);
    }
}

[BurstCompile]
[WithAll(typeof(FlowFieldFollower))]
public partial struct TestCanMoveStraightJob : IJobEntity {

    [NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldFollower> flowFieldFollowerComponentLookup;
    [ReadOnly] public CollisionWorld collisionWorld;

    public void Execute(
        in LocalTransform localTransform,
        ref UnitMover unitMover,
        Entity entity) {

        FlowFieldFollower flowFieldFollower = flowFieldFollowerComponentLookup[entity];

        RaycastInput raycastInput = new RaycastInput {
            Start = localTransform.Position,
            End = flowFieldFollower.targetPosition,
            Filter = new CollisionFilter {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.PATHFINDING_WALLS,
                GroupIndex = 0
            }
        };

        if (!collisionWorld.CastRay(raycastInput)) {
            // Did not hit anything, no walls in between
            unitMover.targetPosition = flowFieldFollower.targetPosition;
            flowFieldFollowerComponentLookup.SetComponentEnabled(entity, false);
        }
    }
}

[BurstCompile]
[WithAll(typeof(FlowFieldFollower))]
public partial struct FlowFieldFollowerJob : IJobEntity {

    [NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldFollower> flowFieldFollowerComponentLookup;

    [ReadOnly] public ComponentLookup<GridSystem.GridNode> gridNodeComponentLookup;
    [ReadOnly] public float gridNodeSize;
    [ReadOnly] public float gridNodeSizeDouble;
    [ReadOnly] public int width;
    [ReadOnly] public int height;
    [ReadOnly] public NativeArray<Entity> totalGridMapEntityArray;

    public void Execute(
        in LocalTransform localTransform,
        ref UnitMover unitMover,
        Entity entity) {
        
        FlowFieldFollower flowFieldFollower = flowFieldFollowerComponentLookup[entity];
        int2 gridPosition = GridSystem.GetGridPosition(localTransform.Position, gridNodeSize);
        int index = GridSystem.CalculateIndex(gridPosition, width);
        int totalCount = width * height;
        Entity gridNodeEntity = totalGridMapEntityArray[totalCount * flowFieldFollower.gridIndex + index];
        GridSystem.GridNode gridNode = gridNodeComponentLookup[gridNodeEntity];
        float3 gridNodeMoveVector = GridSystem.GetWorldMovementVector(gridNode.vector);

        if (GridSystem.IsWall(gridNode)) {
            gridNodeMoveVector = flowFieldFollower.lastMoveVector;
        } else {
            flowFieldFollower.lastMoveVector = gridNodeMoveVector;
        }

        unitMover.targetPosition =
            GridSystem.GetWorldCenterPosition(gridPosition.x, gridPosition.y, gridNodeSize) +
            gridNodeMoveVector *
            gridNodeSizeDouble;

        if (math.distance(localTransform.Position, flowFieldFollower.targetPosition) < gridNodeSize) {
            // Target destination
            unitMover.targetPosition = localTransform.Position;
            flowFieldFollowerComponentLookup.SetComponentEnabled(entity, false);
        }

        flowFieldFollowerComponentLookup[entity] = flowFieldFollower;
    }
}