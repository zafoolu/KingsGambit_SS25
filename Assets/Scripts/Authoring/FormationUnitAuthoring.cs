using Unity.Entities;
using UnityEngine;

public class FormationUnitAuthoring : MonoBehaviour
{
    public GameObject standardBearer; // Referenz auf den Fahnenträger
    public int formationIndex;        // Position in der Formation

    public class Baker : Baker<FormationUnitAuthoring>
    {
        public override void Bake(FormationUnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var standardBearerEntity = GetEntity(authoring.standardBearer, TransformUsageFlags.Dynamic);

            AddComponent(entity, new FormationUnit
            {
                StandardBearerEntity = standardBearerEntity,
                FormationIndex = authoring.formationIndex
            });
        }
    }

    public struct FormationUnit : IComponentData
    {
        public Entity StandardBearerEntity;
        public int FormationIndex;  // Position in der Formation (0-15 für 4x4)
    }
}