using Unity.Entities;
using UnityEngine;

public class CapturePointAuthoring : MonoBehaviour {

    public float captureRadius = 5f;
    public float captureTime = 5f;
    public FactionType controllingFaction = FactionType.None;  // Standardmäßig neutral
    public CapturePointUI uiPrefab;  // Referenz auf das UI-Prefab

    private void Start() {
        if (uiPrefab != null) {
            var uiInstance = Instantiate(uiPrefab, transform.position, Quaternion.identity);
            uiInstance.transform.SetParent(transform);
            
            // Hole die Entity über den EntityManager
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entities = entityManager.GetAllEntities();
            
            foreach (var entity in entities) {
                if (entityManager.HasComponent<CapturePoint>(entity)) {
                    uiInstance.SetCapturePointEntity(entity);
                    break;
                }
            }
            
            entities.Dispose();
        }
    }

    public class Baker : Baker<CapturePointAuthoring> {
        public override void Bake(CapturePointAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new CapturePoint {
                radius = authoring.captureRadius,
                timeToCapture = authoring.captureTime,
                currentCaptureTime = 0f,
                isCaptured = false,
                controllingFaction = authoring.controllingFaction
            });
        }
    }
}

public struct CapturePoint : IComponentData {
    public float radius;
    public float timeToCapture;
    public float currentCaptureTime;
    public bool isCaptured;
    public FactionType controllingFaction;
}