using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class UnitMoverAuthoring : MonoBehaviour {

    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    
    [Header("Collision Avoidance")]
    public float personalSpace = 1.5f;       // Minimaler Abstand zu anderen Einheiten
    public float avoidanceWeight = 0.7f;     // Wie stark die Einheit ausweicht (0-1)

    public class Baker : Baker<UnitMoverAuthoring> {

        public override void Bake(UnitMoverAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitMover {
                moveSpeed = authoring.moveSpeed,
                rotationSpeed = authoring.rotationSpeed,
                personalSpace = authoring.personalSpace,
                avoidanceWeight = authoring.avoidanceWeight,
                avoidanceDirection = float3.zero,
                isMoving = false
            });
        }
    }
}
