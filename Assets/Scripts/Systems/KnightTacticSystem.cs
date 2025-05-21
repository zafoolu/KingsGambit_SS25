using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;

public partial class KnightTacticSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var entityPositions = new NativeList<EntityPosition>(Allocator.TempJob);
        
        try
        {
            
            Entities
                .WithName("CollectEntityPositions")
                .WithAll<LocalToWorld, ShootVictim>() // Nur Entities mit ShootVictim-Komponente
                .ForEach((Entity entity, in LocalToWorld transform) =>
                {
                    entityPositions.Add(new EntityPosition 
                    { 
                        Entity = entity, 
                        Position = transform.Position 
                    });
                })
                .Run();

            
            Entities
                .WithName("CheckTacticOverlaps")
                .ForEach((Entity entity, in KnightTactic tactic, in LocalToWorld transform) =>
                {
                    float3 worldPos = transform.Position;
                    quaternion worldRot = transform.Rotation;

                    float3 hitbox1WorldPos = worldPos + math.mul(worldRot, new float3(tactic.collider1Position.x, tactic.collider1Position.y, tactic.collider1Position.z));
                    quaternion hitbox1WorldRot = math.mul(worldRot, tactic.collider1Rotation);

                    float3 hitbox2WorldPos = worldPos + math.mul(worldRot, new float3(tactic.collider2Position.x, tactic.collider2Position.y, tactic.collider2Position.z));
                    quaternion hitbox2WorldRot = math.mul(worldRot, tactic.collider2Rotation);

                    #if UNITY_EDITOR
                    int collider1Hits = 0;
                    int collider2Hits = 0;
                    Entity collider1Entity = Entity.Null;
                    Entity collider2Entity = Entity.Null;

                    // Zähle zuerst die Kollisionen
                    for (int i = 0; i < entityPositions.Length; i++)
                    {
                        var otherEntityPos = entityPositions[i];
                        if (otherEntityPos.Entity == entity) continue;

                        bool inCollider1 = IsPointInBox(otherEntityPos.Position, hitbox1WorldPos, hitbox1WorldRot, tactic.collider1Size);
                        bool inCollider2 = IsPointInBox(otherEntityPos.Position, hitbox2WorldPos, hitbox2WorldRot, tactic.collider2Size);

                        if (inCollider1)
                        {
                            collider1Hits++;
                            collider1Entity = otherEntityPos.Entity;
                        }
                        if (inCollider2)
                        {
                            collider2Hits++;
                            collider2Entity = otherEntityPos.Entity;
                        }
                    }

                    // Setze die Farben basierend auf der Anzahl der Kollisionen
                    Color box1Color = collider1Hits == 1 ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
                    Color box2Color = collider2Hits == 1 ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);

                    DrawDebugBox(hitbox1WorldPos, hitbox1WorldRot, tactic.collider1Size, box1Color);
                    DrawDebugBox(hitbox2WorldPos, hitbox2WorldRot, tactic.collider2Size, box2Color);

                    // Debug-Ausgaben
                    if (collider1Hits == 1 && collider2Hits == 1)
                    {
                        Debug.Log($"SHOOT - Perfekte Taktik! Entity {entity.Index} mit Victims {collider1Entity.Index} und {collider2Entity.Index}");
                    }
                    else
                    {
                        if (collider1Hits > 0)
                        {
                            Debug.Log($"Box 1 hat {collider1Hits} Kollision(en)");
                        }
                        if (collider2Hits > 0)
                        {
                            Debug.Log($"Box 2 hat {collider2Hits} Kollision(en)");
                        }
                    }
                    #endif
                })
                .Run();
        }
        finally
        {
            entityPositions.Dispose();
        }
    }

    private struct EntityPosition
    {
        public Entity Entity;
        public float3 Position;
    }

    private static bool IsPointInBox(float3 point, float3 boxPosition, quaternion boxRotation, Vector3 boxSize)
    {
        // Transformiere den Punkt in lokale Koordinaten der Box
        float3 localPoint = math.mul(math.inverse(boxRotation), point - boxPosition);
        
        // Prüfe ob der Punkt innerhalb der Box-Grenzen liegt
        Vector3 halfSize = boxSize * 0.5f;
        return math.abs(localPoint.x) <= halfSize.x &&
               math.abs(localPoint.y) <= halfSize.y &&
               math.abs(localPoint.z) <= halfSize.z;
    }

    private static void DrawDebugBox(float3 position, quaternion rotation, Vector3 size, Color color)
    {
        Vector3 pos = new Vector3(position.x, position.y, position.z);
        Quaternion rot = new Quaternion(rotation.value.x, rotation.value.y, rotation.value.z, rotation.value.w);

        Vector3[] points = new Vector3[8];
        Vector3 halfSize = size * 0.5f;

        points[0] = pos + rot * new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
        points[1] = pos + rot * new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
        points[2] = pos + rot * new Vector3(halfSize.x, -halfSize.y, halfSize.z);
        points[3] = pos + rot * new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
        points[4] = pos + rot * new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
        points[5] = pos + rot * new Vector3(halfSize.x, halfSize.y, -halfSize.z);
        points[6] = pos + rot * new Vector3(halfSize.x, halfSize.y, halfSize.z);
        points[7] = pos + rot * new Vector3(-halfSize.x, halfSize.y, halfSize.z);

        Debug.DrawLine(points[0], points[1], color);
        Debug.DrawLine(points[1], points[2], color);
        Debug.DrawLine(points[2], points[3], color);
        Debug.DrawLine(points[3], points[0], color);

        Debug.DrawLine(points[4], points[5], color);
        Debug.DrawLine(points[5], points[6], color);
        Debug.DrawLine(points[6], points[7], color);
        Debug.DrawLine(points[7], points[4], color);

        Debug.DrawLine(points[0], points[4], color);
        Debug.DrawLine(points[1], points[5], color);
        Debug.DrawLine(points[2], points[6], color);
        Debug.DrawLine(points[3], points[7], color);
    }
}