using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct RookTacticSystem : ISystem
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
        
        foreach (var (rookTactic, transform, entity) in 
                 SystemAPI.Query<RefRW<RookTactic>, RefRO<LocalTransform>>()
                 .WithEntityAccess())
        {
            rookTactic.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            
            // Berechne Hitbox Position mit dem konfigurierbaren Offset
            float3 entityPosition = transform.ValueRO.Position;
            quaternion entityRotation = transform.ValueRO.Rotation;
            
            // Transformiere den lokalen Offset in Weltkoordinaten
            float3 worldOffset = math.mul(entityRotation, rookTactic.ValueRO.hitboxOffset);
            float3 hitboxCenter = entityPosition + worldOffset;
            
            // Erstelle Hitbox
            float3 hitboxSize = new float3(
                rookTactic.ValueRO.hitboxWidth,
                rookTactic.ValueRO.hitboxHeight,
                rookTactic.ValueRO.hitboxDepth
            );
            
            // Suche nach Zielen in der Hitbox
            var targetsInHitbox = GetTargetsInBox(collisionWorld, hitboxCenter, hitboxSize, entityRotation);
            
            // Prüfe auf FlagBearer-Kollisionen
            bool isCollidingWithFlagBearer = CheckForFlagBearerCollision(ref state, targetsInHitbox);
            
            // Erstelle oder aktualisiere CollisionState Komponente
            if (!SystemAPI.HasComponent<TacticCollisionState>(entity))
            {
                ecb.AddComponent(entity, new TacticCollisionState
                {
                    isCollidingWithFlagBearer = isCollidingWithFlagBearer,
                    originalColor = rookTactic.ValueRO.hitboxColor,
                    collisionColor = new float4(0f, 1f, 0f, rookTactic.ValueRO.hitboxColor.w) // Grün
                });
            }
            else
            {
                var collisionState = SystemAPI.GetComponent<TacticCollisionState>(entity);
                collisionState.isCollidingWithFlagBearer = isCollidingWithFlagBearer;
                ecb.SetComponent(entity, collisionState);
            }
            
            if (rookTactic.ValueRO.timer <= 0f)
            {
                if (targetsInHitbox.Length > 0)
                {
                    Debug.Log($"🎯 ROOK TACTIC: Found {targetsInHitbox.Length} targets in hitbox at position {hitboxCenter}!");
                    
                    if (isCollidingWithFlagBearer)
                    {
                        Debug.Log($"💚 ROOK TACTIC: Colliding with FlagBearer! Hitbox should be GREEN!");
                    }
                    
                    // Hier kannst du die Logik für den Rook-Angriff implementieren
                    // Zum Beispiel Schaden verursachen oder andere Effekte
                    
                    rookTactic.ValueRW.timer = rookTactic.ValueRO.timerMax;
                }
                else
                {
                    rookTactic.ValueRW.timer = rookTactic.ValueRO.timerMax;
                }
            }
            
            targetsInHitbox.Dispose();
        }
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
    
    [BurstCompile]
    private NativeList<Entity> GetTargetsInBox(CollisionWorld collisionWorld, float3 center, float3 size, quaternion rotation)
    {
        var targets = new NativeList<Entity>(Allocator.Temp);
        var distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);
        
        // Verwende DOTS Physics OverlapBox
        if (collisionWorld.OverlapBox(
            center,
            rotation,
            size * 0.5f, // OverlapBox erwartet half-extents
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
            }
        }
        
        distanceHitList.Dispose();
        return targets;
    }
    
    [BurstCompile]
    private bool CheckForFlagBearerCollision(ref SystemState state, NativeList<Entity> targets)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            if (SystemAPI.HasComponent<FlagBearer>(targets[i]))
            {
                return true;
            }
        }
        return false;
    }
}