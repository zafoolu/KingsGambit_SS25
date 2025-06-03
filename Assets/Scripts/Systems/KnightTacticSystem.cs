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
        Vector3 halfSize = size * 0.5f;

        // Draw the bottom face
        Vector3 p0 = pos + rot * new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
        Vector3 p1 = pos + rot * new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
        Vector3 p2 = pos + rot * new Vector3(halfSize.x, -halfSize.y, halfSize.z);
        Vector3 p3 = pos + rot * new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
        
        // Draw the top face
        Vector3 p4 = pos + rot * new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
        Vector3 p5 = pos + rot * new Vector3(halfSize.x, halfSize.y, -halfSize.z);
        Vector3 p6 = pos + rot * new Vector3(halfSize.x, halfSize.y, halfSize.z);
        Vector3 p7 = pos + rot * new Vector3(-halfSize.x, halfSize.y, halfSize.z);

        // Draw bottom face
        Debug.DrawLine(p0, p1, color);
        Debug.DrawLine(p1, p2, color);
        Debug.DrawLine(p2, p3, color);
        Debug.DrawLine(p3, p0, color);

        // Draw top face
        Debug.DrawLine(p4, p5, color);
        Debug.DrawLine(p5, p6, color);
        Debug.DrawLine(p6, p7, color);
        Debug.DrawLine(p7, p4, color);

        // Draw vertical lines
        Debug.DrawLine(p0, p4, color);
        Debug.DrawLine(p1, p5, color);
        Debug.DrawLine(p2, p6, color);
        Debug.DrawLine(p3, p7, color);
    }
}