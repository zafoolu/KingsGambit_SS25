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

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        var collisionWorld = physicsWorldSingleton.CollisionWorld;
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        
        foreach (var (knightTactic, transform, entity) in 
                 SystemAPI.Query<RefRW<KnightTactic>, RefRO<LocalTransform>>()
                 .WithEntityAccess())
        {
            knightTactic.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            
            // Berechne Hitbox 1 Position, Größe und Rotation
            float3 hitbox1WorldPos = transform.ValueRO.Position + math.mul(transform.ValueRO.Rotation, knightTactic.ValueRO.hitbox1Offset);
            float3 hitbox1Size = new float3(knightTactic.ValueRO.hitbox1Width, knightTactic.ValueRO.hitbox1Height, knightTactic.ValueRO.hitbox1Depth);
            quaternion hitbox1Rotation = math.mul(transform.ValueRO.Rotation, quaternion.Euler(math.radians(knightTactic.ValueRO.hitbox1Rotation)));
            
            // Berechne Hitbox 2 Position, Größe und Rotation
            float3 hitbox2WorldPos = transform.ValueRO.Position + math.mul(transform.ValueRO.Rotation, knightTactic.ValueRO.hitbox2Offset);
            float3 hitbox2Size = new float3(knightTactic.ValueRO.hitbox2Width, knightTactic.ValueRO.hitbox2Height, knightTactic.ValueRO.hitbox2Depth);
            quaternion hitbox2Rotation = math.mul(transform.ValueRO.Rotation, quaternion.Euler(math.radians(knightTactic.ValueRO.hitbox2Rotation)));
            
            // Finde Targets in beiden Hitboxen
            var targetsInBox1 = GetTargetsInBox(ref state, collisionWorld, hitbox1WorldPos, hitbox1Size, hitbox1Rotation);
            var targetsInBox2 = GetTargetsInBox(ref state, collisionWorld, hitbox2WorldPos, hitbox2Size, hitbox2Rotation);
            
            // Prüfe ob genau ein FlagBearer in jeder Box ist
            bool hasExactlyOneFlagBearerPerBox = (targetsInBox1.Length == 1 && targetsInBox2.Length == 1);
            
            // Aktualisiere Kollisionsstatus kontinuierlich
            if (!SystemAPI.HasComponent<TacticCollisionState>(entity))
            {
                ecb.AddComponent(entity, new TacticCollisionState
                {
                    isCollidingWithFlagBearer = hasExactlyOneFlagBearerPerBox,
                    originalColor = knightTactic.ValueRO.hitboxColor,
                    collisionColor = new float4(0f, 1f, 0f, 0.5f) // Grün für erfolgreiche Aktivierung
                });
            }
            else
            {
                ecb.SetComponent(entity, new TacticCollisionState
                {
                    isCollidingWithFlagBearer = hasExactlyOneFlagBearerPerBox,
                    originalColor = knightTactic.ValueRO.hitboxColor,
                    collisionColor = new float4(0f, 1f, 0f, 0.5f) // Grün für erfolgreiche Aktivierung
                });
            }
            
            if (knightTactic.ValueRO.timer <= 0f)
            {
                if (hasExactlyOneFlagBearerPerBox)
                {
                    Debug.Log($"💚 KNIGHT TACTIC: EXACTLY 1 FlagBearer per hitbox! Activating tactic!");
                    
                    // Knight Tactic aktiviert!
                    knightTactic.ValueRW.onShoot.isTriggered = true;
                    knightTactic.ValueRW.onShoot.shootFromPosition = transform.ValueRO.Position;
                    
                    // Schade den Targets
                    Entity target1 = targetsInBox1[0];
                    Entity target2 = targetsInBox2[0];
                    
                    if (SystemAPI.HasComponent<Health>(target1))
                    {
                        var health1 = SystemAPI.GetComponent<Health>(target1);
                        health1.healthAmount -= knightTactic.ValueRO.damageAmount;
                        ecb.SetComponent(target1, health1);
                    }
                    
                    if (SystemAPI.HasComponent<Health>(target2))
                    {
                        var health2 = SystemAPI.GetComponent<Health>(target2);
                        health2.healthAmount -= knightTactic.ValueRO.damageAmount;
                        ecb.SetComponent(target2, health2);
                    }
                }
                else
                {
                    Debug.Log($"🎯 KNIGHT TACTIC: Found {targetsInBox1.Length} targets in box1, {targetsInBox2.Length} targets in box2. Need exactly 1 per box!");
                }
                
                knightTactic.ValueRW.timer = knightTactic.ValueRO.timerMax;
            }
            
            targetsInBox1.Dispose();
            targetsInBox2.Dispose();
        }
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
    
    [BurstCompile]
    private NativeList<Entity> GetTargetsInBox(ref SystemState state, CollisionWorld collisionWorld, float3 boxCenter, float3 boxSize, quaternion boxRotation)
    {
        var targets = new NativeList<Entity>(Allocator.Temp);
        var distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);
        
        float3 halfExtents = boxSize * 0.5f;
        
        if (collisionWorld.OverlapBox(
            boxCenter,
            boxRotation,
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
                // Prüfe ob es ein FlagBearer ist
                if (state.EntityManager.HasComponent<FlagBearer>(distanceHit.Entity))
                {
                    targets.Add(distanceHit.Entity);
                }
            }
        }
        
        distanceHitList.Dispose();
        return targets;
    }
}