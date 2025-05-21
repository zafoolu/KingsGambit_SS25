using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
        var deltaTime = SystemAPI.Time.DeltaTime;
        var entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        foreach ((RefRW<LocalTransform> transform, RefRW<KnightTactic> knightTactic) 
            in SystemAPI.Query<RefRW<LocalTransform>, RefRW<KnightTactic>>())
        {
            // Timer aktualisieren
            knightTactic.ValueRW.timer += deltaTime;
            if (knightTactic.ValueRW.timer < knightTactic.ValueRO.timerMax) continue;

            int targetsInBox1 = 0;
            int targetsInBox2 = 0;
            Entity targetInBox1 = Entity.Null;
            Entity targetInBox2 = Entity.Null;

            foreach ((RefRO<LocalTransform> victimTransform, Entity victimEntity) 
                in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<ShootVictim>().WithEntityAccess())
            {
                float3 localPos = victimTransform.ValueRO.Position - transform.ValueRO.Position;

                if (IsPointInBox(localPos, knightTactic.ValueRO.collider1Position, 
                    knightTactic.ValueRO.collider1Size, knightTactic.ValueRO.collider1Rotation))
                {
                    targetsInBox1++;
                    targetInBox1 = victimEntity;
                }

                if (IsPointInBox(localPos, knightTactic.ValueRO.collider2Position, 
                    knightTactic.ValueRO.collider2Size, knightTactic.ValueRO.collider2Rotation))
                {
                    targetsInBox2++;
                    targetInBox2 = victimEntity;
                }
            }

            if (targetsInBox1 == 1 && targetsInBox2 == 1)
            {
                Debug.Log("SHOOT - Perfekte Taktik!");
                
                // Timer zurücksetzen
                knightTactic.ValueRW.timer = 0f;

                // Schuss-Event auslösen
                knightTactic.ValueRW.onShoot.isTriggered = true;
                knightTactic.ValueRW.onShoot.shootFromPosition = transform.ValueRO.Position;

                // Bullets erstellen
                CreateBullet(state, entitiesReferences, transform.ValueRO.Position, targetInBox1);
                CreateBullet(state, entitiesReferences, transform.ValueRO.Position, targetInBox2);
            }
        }
    }

    private void CreateBullet(SystemState state, EntitiesReferences references, float3 position, Entity target)
    {
        Entity bullet = state.EntityManager.Instantiate(references.bulletPrefabEntity);
        SystemAPI.SetComponent(bullet, LocalTransform.FromPosition(position));
        SystemAPI.GetComponentRW<Target>(bullet).ValueRW.targetEntity = target;
    }

    private bool IsPointInBox(float3 point, float3 boxPosition, float3 boxSize, quaternion boxRotation)
    {
        float3 localPoint = math.mul(math.inverse(boxRotation), point - boxPosition);
        return math.abs(localPoint.x) <= boxSize.x / 2 &&
               math.abs(localPoint.y) <= boxSize.y / 2 &&
               math.abs(localPoint.z) <= boxSize.z / 2;
    }
}