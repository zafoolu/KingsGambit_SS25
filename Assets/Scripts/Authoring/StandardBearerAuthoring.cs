using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class StandardBearerAuthoring : MonoBehaviour
{
    public int formationWidth = 4;
    public int formationHeight = 4;
    public float unitSpacing = 2f;

    public class Baker : Baker<StandardBearerAuthoring>
    {
        public override void Bake(StandardBearerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new StandardBearer
            {
                Entity = entity,
                Position = authoring.transform.position,
                FormationWidth = authoring.formationWidth,
                FormationHeight = authoring.formationHeight,
                UnitSpacing = authoring.unitSpacing
            });
        }
    }

    public struct StandardBearer : IComponentData
    {
        public Entity Entity;
        public float3 Position;
        public int FormationWidth;  // Breite der Formation (4 für 4x4)
        public int FormationHeight; // Höhe der Formation (4 für 4x4)
        public float UnitSpacing;   // Abstand zwischen den Einheiten
    }
}