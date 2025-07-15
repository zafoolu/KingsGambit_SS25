using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct KnightTacticSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
    }

    // NICHT BurstCompile, weil wir auf GameObjects zugreifen
    public void OnUpdate(ref SystemState state)
    {
        var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        var collisionWorld = physicsWorldSingleton.CollisionWorld;
        
        foreach (var (knightTactic, transform, entity) in 
                 SystemAPI.Query<RefRW<KnightTactic>, RefRO<LocalTransform>>()
                 .WithEntityAccess())
        {
            knightTactic.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            
            if (knightTactic.ValueRO.timer <= 0f)
            {
                Debug.Log($"\n=== Knight Tactic Check for Entity {entity.Index} ===");
                
                // Prüfe auf LocalTransform statt Transform
                if (SystemAPI.HasComponent<LocalTransform>(knightTactic.ValueRO.tacticBox1Entity) &&
                    SystemAPI.HasComponent<LocalTransform>(knightTactic.ValueRO.tacticBox2Entity))
                {
                    var box1Transform = SystemAPI.GetComponent<LocalTransform>(knightTactic.ValueRO.tacticBox1Entity);
                    var box2Transform = SystemAPI.GetComponent<LocalTransform>(knightTactic.ValueRO.tacticBox2Entity);
                    
                    Debug.Log($"Box1 Position: {box1Transform.Position}, Box2 Position: {box2Transform.Position}");
                    
                    // Verwende DOTS Physics für Collision Detection
                    var targetsInBox1 = GetTargetsInBoxEntity(ref state, collisionWorld, knightTactic.ValueRO.tacticBox1Entity);
                    var targetsInBox2 = GetTargetsInBoxEntity(ref state, collisionWorld, knightTactic.ValueRO.tacticBox2Entity);
                    
                    // Prüfe ob genau ein Ziel in jeder Box ist
                    Debug.Log($"Targets in Box1: {targetsInBox1.Length}, Targets in Box2: {targetsInBox2.Length}");
                    
                    if (targetsInBox1.Length == 1 && targetsInBox2.Length == 1)
                    {
                        knightTactic.ValueRW.onShoot.isTriggered = true;
                        knightTactic.ValueRW.timer = knightTactic.ValueRO.timerMax;
                        
                        Debug.Log($"🎯 KNIGHT TACTIC ACTIVATED! Target1: {targetsInBox1[0].Index}, Target2: {targetsInBox2[0].Index}");
                    }
                    else
                    {
                        Debug.Log($"❌ Knight Tactic NOT activated - need exactly 1 target in each box");
                    }
                    
                    targetsInBox1.Dispose();
                    targetsInBox2.Dispose();
                }
                else
                {
                    Debug.LogWarning($"Knight Entity {entity.Index} missing tactic box LocalTransform components!");
                }
                
                Debug.Log($"=== End Knight Tactic Check ===\n");
            }
        }
    }
    
    // Methode die DOTS Physics verwendet
    private NativeList<Entity> GetTargetsInBoxEntity(ref SystemState state, CollisionWorld collisionWorld, 
        Entity boxEntity)
    {
        var targets = new NativeList<Entity>(Allocator.Temp);
        
        // Hole das echte GameObject über ManagedAPI
        if (SystemAPI.ManagedAPI.HasComponent<Transform>(boxEntity))
        {
            var gameObjectTransform = SystemAPI.ManagedAPI.GetComponent<Transform>(boxEntity);
            var boxCollider = gameObjectTransform.GetComponent<UnityEngine.BoxCollider>();
            if (boxCollider != null)
            {
                // ✅ RICHTIG: Verwende die echten BoxCollider-Dimensionen
                Vector3 center = gameObjectTransform.position + gameObjectTransform.TransformVector(boxCollider.center);
                Vector3 halfExtents = Vector3.Scale(boxCollider.size, gameObjectTransform.lossyScale) * 0.5f;
                Quaternion orientation = gameObjectTransform.rotation;
                
                Debug.Log($"Using REAL BoxCollider: center={center}, halfExtents={halfExtents}");
                
                // Verwende DOTS Physics OverlapBox mit ECHTEN Dimensionen
                var distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);
                
                if (collisionWorld.OverlapBox(
                    center,
                    orientation,
                    halfExtents,
                    ref distanceHitList,
                    new CollisionFilter {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << GameAssets.UNITS_LAYER,
                        GroupIndex = 0,
                    }))
                {
                    foreach (var distanceHit in distanceHitList)
                    {
                        targets.Add(distanceHit.Entity);
                        Debug.Log($"Found entity {distanceHit.Entity.Index} with REAL box dimensions");
                    }
                }
                
                distanceHitList.Dispose();
            }
            else
            {
                Debug.LogError($"Box Entity {boxEntity.Index} has no BoxCollider component!");
            }
        }
        else
        {
            Debug.LogError($"Box Entity {boxEntity.Index} has no Transform component!");
        }
        
        return targets;
    }
    
    [BurstCompile]
    private bool IsPointInOrientedBox(float3 point, float3 boxCenter, float3 boxSize, quaternion boxRotation)
    {
        // Transformiere Punkt in lokale Box-Koordinaten
        float3 localPoint = math.mul(math.inverse(boxRotation), point - boxCenter);
        
        // Prüfe ob Punkt innerhalb der Box-Grenzen liegt
        float3 halfSize = boxSize * 0.5f;
        return math.abs(localPoint.x) <= halfSize.x && 
               math.abs(localPoint.y) <= halfSize.y && 
               math.abs(localPoint.z) <= halfSize.z;
    }
}