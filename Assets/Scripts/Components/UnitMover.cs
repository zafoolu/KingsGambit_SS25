using Unity.Entities;
using Unity.Mathematics;

public struct UnitMover : IComponentData {
    public float moveSpeed;
    public float rotationSpeed;
    public float3 targetPosition;
    public bool isMoving;

    // Kollisionsvermeidungsparameter
    public float3 avoidanceDirection;    // Aktuelle Ausweichrichtung
    public float avoidanceWeight;        // Gewichtung der Ausweichbewegung (0-1)
    public float personalSpace;          // Minimaler gewünschter Abstand zu anderen Einheiten
}