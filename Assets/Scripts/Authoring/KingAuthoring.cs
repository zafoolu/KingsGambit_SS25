using Unity.Entities;
using UnityEngine;

public class KingAuthoring : MonoBehaviour {

    public class Baker : Baker<KingAuthoring> {

        public override void Bake(KingAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new King {
            });
        }

    }

}


public struct King : IComponentData {



}