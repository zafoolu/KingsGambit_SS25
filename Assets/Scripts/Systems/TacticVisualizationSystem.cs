using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct TacticVisualizationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
    }
    
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        
        // Erstelle Visual Entities für neue RookTactic Entities
        foreach (var (rookTactic, transform, entity) in 
                 SystemAPI.Query<RefRO<RookTactic>, RefRO<LocalTransform>>()
                     .WithEntityAccess()
                     .WithNone<RookTacticVisualInstance>())
        {
            if (rookTactic.ValueRO.showRuntimeVisual)
            {
                // Berechne Hitbox Position
                float3 hitboxWorldPos = transform.ValueRO.Position + math.rotate(transform.ValueRO.Rotation, rookTactic.ValueRO.hitboxOffset);
                
                // Bestimme Farbe basierend auf Kollisionsstatus
                float4 currentColor = rookTactic.ValueRO.hitboxColor;
                if (SystemAPI.HasComponent<TacticCollisionState>(entity))
                {
                    var collisionState = SystemAPI.GetComponent<TacticCollisionState>(entity);
                    currentColor = collisionState.isCollidingWithFlagBearer ? collisionState.collisionColor : collisionState.originalColor;
                }
                
                // Erstelle Visual Entity
                Entity visualEntity = ecb.CreateEntity();
                ecb.AddComponent(visualEntity, new LocalTransform
                {
                    Position = hitboxWorldPos,
                    Rotation = transform.ValueRO.Rotation,
                    Scale = 1f
                });
                
                ecb.AddComponent(visualEntity, new TacticVisualMarker
                {
                    color = currentColor,
                    size = new float3(rookTactic.ValueRO.hitboxWidth, rookTactic.ValueRO.hitboxHeight, rookTactic.ValueRO.hitboxDepth)
                });
                
                // Verknüpfe Visual Entity mit Parent Entity
                ecb.AddComponent(entity, new RookTacticVisualInstance
                {
                    visualEntity = visualEntity
                });
            }
        }
        
        // Erstelle Visual Entities für neue KnightTactic Entities
        foreach (var (knightTactic, transform, entity) in 
                 SystemAPI.Query<RefRO<KnightTactic>, RefRO<LocalTransform>>()
                     .WithEntityAccess()
                     .WithNone<KnightTacticVisualInstance>())
        {
            if (knightTactic.ValueRO.showRuntimeVisual)
            {
                // Bestimme Farbe basierend auf Kollisionsstatus
                float4 currentColor = knightTactic.ValueRO.hitboxColor;
                if (SystemAPI.HasComponent<TacticCollisionState>(entity))
                {
                    var collisionState = SystemAPI.GetComponent<TacticCollisionState>(entity);
                    currentColor = collisionState.isCollidingWithFlagBearer ? collisionState.collisionColor : collisionState.originalColor;
                }
                
                // Erstelle Visual Entity für Hitbox 1
                float3 hitbox1WorldPos = transform.ValueRO.Position + math.rotate(transform.ValueRO.Rotation, knightTactic.ValueRO.hitbox1Offset);
                quaternion hitbox1Rotation = math.mul(transform.ValueRO.Rotation, quaternion.Euler(math.radians(knightTactic.ValueRO.hitbox1Rotation)));
                Entity visual1Entity = ecb.CreateEntity();
                ecb.AddComponent(visual1Entity, new LocalTransform
                {
                    Position = hitbox1WorldPos,
                    Rotation = hitbox1Rotation,
                    Scale = 1f
                });
                ecb.AddComponent(visual1Entity, new TacticVisualMarker
                {
                    color = currentColor,
                    size = new float3(knightTactic.ValueRO.hitbox1Width, knightTactic.ValueRO.hitbox1Height, knightTactic.ValueRO.hitbox1Depth)
                });
                
                // Erstelle Visual Entity für Hitbox 2
                float3 hitbox2WorldPos = transform.ValueRO.Position + math.rotate(transform.ValueRO.Rotation, knightTactic.ValueRO.hitbox2Offset);
                quaternion hitbox2Rotation = math.mul(transform.ValueRO.Rotation, quaternion.Euler(math.radians(knightTactic.ValueRO.hitbox2Rotation)));
                Entity visual2Entity = ecb.CreateEntity();
                ecb.AddComponent(visual2Entity, new LocalTransform
                {
                    Position = hitbox2WorldPos,
                    Rotation = hitbox2Rotation,
                    Scale = 1f
                });
                ecb.AddComponent(visual2Entity, new TacticVisualMarker
                {
                    color = currentColor,
                    size = new float3(knightTactic.ValueRO.hitbox2Width, knightTactic.ValueRO.hitbox2Height, knightTactic.ValueRO.hitbox2Depth)
                });
                
                // Verknüpfe Visual Entities mit Parent Entity
                ecb.AddComponent(entity, new KnightTacticVisualInstance
                {
                    visual1Entity = visual1Entity,
                    visual2Entity = visual2Entity
                });
            }
        }
        
        // Update bestehende RookTactic Visual Entities
        var entityManager = state.EntityManager;
        foreach (var (rookTactic, transform, visualInstance, entity) in 
                 SystemAPI.Query<RefRO<RookTactic>, RefRO<LocalTransform>, RefRO<RookTacticVisualInstance>>()
                     .WithEntityAccess())
        {
            if (entityManager.Exists(visualInstance.ValueRO.visualEntity))
            {
                // Berechne neue Position
                float3 hitboxWorldPos = transform.ValueRO.Position + math.rotate(transform.ValueRO.Rotation, rookTactic.ValueRO.hitboxOffset);
                
                // Bestimme Farbe basierend auf Kollisionsstatus
                float4 currentColor = rookTactic.ValueRO.hitboxColor;
                if (SystemAPI.HasComponent<TacticCollisionState>(entity))
                {
                    var collisionState = SystemAPI.GetComponent<TacticCollisionState>(entity);
                    currentColor = collisionState.isCollidingWithFlagBearer ? collisionState.collisionColor : collisionState.originalColor;
                }
                
                // Update Visual Entity Transform
                entityManager.SetComponentData(visualInstance.ValueRO.visualEntity, new LocalTransform
                {
                    Position = hitboxWorldPos,
                    Rotation = transform.ValueRO.Rotation,
                    Scale = 1f
                });
                
                // Update Visual Marker mit aktueller Farbe
                entityManager.SetComponentData(visualInstance.ValueRO.visualEntity, new TacticVisualMarker
                {
                    color = currentColor,
                    size = new float3(rookTactic.ValueRO.hitboxWidth, rookTactic.ValueRO.hitboxHeight, rookTactic.ValueRO.hitboxDepth)
                });
            }
        }
        
        // Update bestehende KnightTactic Visual Entities
        foreach (var (knightTactic, transform, visualInstance, entity) in 
                 SystemAPI.Query<RefRO<KnightTactic>, RefRO<LocalTransform>, RefRO<KnightTacticVisualInstance>>()
                     .WithEntityAccess())
        {
            // Bestimme Farbe basierend auf Kollisionsstatus
            float4 currentColor = knightTactic.ValueRO.hitboxColor;
            if (SystemAPI.HasComponent<TacticCollisionState>(entity))
            {
                var collisionState = SystemAPI.GetComponent<TacticCollisionState>(entity);
                currentColor = collisionState.isCollidingWithFlagBearer ? collisionState.collisionColor : collisionState.originalColor;
            }
            
            // Update Visual Entity 1
            if (entityManager.Exists(visualInstance.ValueRO.visual1Entity))
            {
                float3 hitbox1WorldPos = transform.ValueRO.Position + math.rotate(transform.ValueRO.Rotation, knightTactic.ValueRO.hitbox1Offset);
                quaternion hitbox1Rotation = math.mul(transform.ValueRO.Rotation, quaternion.Euler(math.radians(knightTactic.ValueRO.hitbox1Rotation)));
                entityManager.SetComponentData(visualInstance.ValueRO.visual1Entity, new LocalTransform
                {
                    Position = hitbox1WorldPos,
                    Rotation = hitbox1Rotation,
                    Scale = 1f
                });
                entityManager.SetComponentData(visualInstance.ValueRO.visual1Entity, new TacticVisualMarker
                {
                    color = currentColor,
                    size = new float3(knightTactic.ValueRO.hitbox1Width, knightTactic.ValueRO.hitbox1Height, knightTactic.ValueRO.hitbox1Depth)
                });
            }
            
            // Update Visual Entity 2
            if (entityManager.Exists(visualInstance.ValueRO.visual2Entity))
            {
                float3 hitbox2WorldPos = transform.ValueRO.Position + math.rotate(transform.ValueRO.Rotation, knightTactic.ValueRO.hitbox2Offset);
                quaternion hitbox2Rotation = math.mul(transform.ValueRO.Rotation, quaternion.Euler(math.radians(knightTactic.ValueRO.hitbox2Rotation)));
                entityManager.SetComponentData(visualInstance.ValueRO.visual2Entity, new LocalTransform
                {
                    Position = hitbox2WorldPos,
                    Rotation = hitbox2Rotation,
                    Scale = 1f
                });
                entityManager.SetComponentData(visualInstance.ValueRO.visual2Entity, new TacticVisualMarker
                {
                    color = currentColor,
                    size = new float3(knightTactic.ValueRO.hitbox2Width, knightTactic.ValueRO.hitbox2Height, knightTactic.ValueRO.hitbox2Depth)
                });
            }
        }
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
    
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        // Cleanup wird automatisch durch Entity Destruction gehandelt
    }
}

// Marker Component für Visual Entities
public struct TacticVisualMarker : IComponentData
{
    public float4 color;
    public float3 size;
}