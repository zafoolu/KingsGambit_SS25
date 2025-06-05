using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class KnightTacticAuthoring : MonoBehaviour
{
    [Header("Collider 1 Einstellungen")]
    public Vector3 collider1Size = Vector3.one;
    public Vector3 collider1Position;
    public Vector3 collider1Rotation;
    public Vector3 collider1Scale = Vector3.one;

    [Header("Collider 2 Einstellungen")]
    public Vector3 collider2Size = Vector3.one;
    public Vector3 collider2Position;
    public Vector3 collider2Rotation;
    public Vector3 collider2Scale = Vector3.one;

    public float timerMax = 1f;  // Neue Variable f�r Schuss-Cooldown
    public int damageAmount = 10; // Neue Variable f�r Schaden

    public class Baker : Baker<KnightTacticAuthoring>
    {
        public override void Bake(KnightTacticAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new KnightTactic
            {
                collider1Size = Vector3.Scale(authoring.collider1Size, authoring.collider1Scale),
                collider2Size = Vector3.Scale(authoring.collider2Size, authoring.collider2Scale),
                collider1Position = authoring.collider1Position,
                collider2Position = authoring.collider2Position,
                collider1Rotation = Quaternion.Euler(authoring.collider1Rotation),
                collider2Rotation = Quaternion.Euler(authoring.collider2Rotation),
                timerMax = authoring.timerMax,
                timer = 0f,
                damageAmount = authoring.damageAmount,
                onShoot = new KnightTactic.OnShootEvent { isTriggered = false }
            });
        }
     }

    private void OnDrawGizmos()
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;

        // Collider 1
        int unitsInCollider1 = CountUnitsInCollider(collider1Position, collider1Size, collider1Rotation, collider1Scale);
        Gizmos.color = new Color(1, 0, 0, 0.5f); // Standard: Rot
        if (unitsInCollider1 == 1)
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f); // Grün wenn genau 1 Unit
        }
        Matrix4x4 hitbox1Matrix = Matrix4x4.TRS(
            transform.position + collider1Position,
            Quaternion.Euler(collider1Rotation),
            Vector3.Scale(collider1Size, collider1Scale)
        );
        Gizmos.matrix = hitbox1Matrix;
        DrawWireBox();

        // Collider 2
        int unitsInCollider2 = CountUnitsInCollider(collider2Position, collider2Size, collider2Rotation, collider2Scale);
        Gizmos.color = new Color(1, 0, 0, 0.5f); // Standard: Rot
        if (unitsInCollider2 == 1)
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f); // Grün wenn genau 1 Unit
        }
        Matrix4x4 hitbox2Matrix = Matrix4x4.TRS(
            transform.position + collider2Position,
            Quaternion.Euler(collider2Rotation),
            Vector3.Scale(collider2Size, collider2Scale)
        );
        Gizmos.matrix = hitbox2Matrix;
        DrawWireBox();

        Gizmos.matrix = originalMatrix;
    }

    private int CountUnitsInCollider(Vector3 position, Vector3 size, Vector3 rotation, Vector3 scale)
    {
        Vector3 scaledSize = Vector3.Scale(size, scale);
        Collider[] colliders = Physics.OverlapBox(
            transform.position + position,
            scaledSize / 2,
            Quaternion.Euler(rotation),
            LayerMask.GetMask("Unit")
        );

        if (colliders == null) return 0;
        return colliders.Length;
    }

    private void DrawWireBox()
    {
        Vector3[] points = new Vector3[8];

        points[0] = new Vector3(-0.5f, -0.5f, -0.5f);
        points[1] = new Vector3(0.5f, -0.5f, -0.5f);
        points[2] = new Vector3(0.5f, -0.5f, 0.5f);
        points[3] = new Vector3(-0.5f, -0.5f, 0.5f);
        points[4] = new Vector3(-0.5f, 0.5f, -0.5f);
        points[5] = new Vector3(0.5f, 0.5f, -0.5f);
        points[6] = new Vector3(0.5f, 0.5f, 0.5f);
        points[7] = new Vector3(-0.5f, 0.5f, 0.5f);

        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(points[i], points[(i + 1) % 4]);
            Gizmos.DrawLine(points[i + 4], points[((i + 1) % 4) + 4]);
            Gizmos.DrawLine(points[i], points[i + 4]);
        }
    }
}

public struct KnightTactic : IComponentData
{
    public Vector3 collider1Size;
    public Vector3 collider2Size;
    public Vector3 collider1Position;
    public Vector3 collider2Position;
    public Quaternion collider1Rotation;
    public Quaternion collider2Rotation;

    public float timer;
    public float timerMax;
    public int damageAmount;
    public OnShootEvent onShoot;

    public struct OnShootEvent
    {
        public bool isTriggered;
        public float3 shootFromPosition;
    }
}