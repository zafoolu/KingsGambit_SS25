using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class CapturePointUI : MonoBehaviour {
    [SerializeField] private Image progressCircleImage;
    [SerializeField] private Image neutralCircleImage;
    
    private Entity capturePointEntity;
    private EntityManager entityManager;
    private bool isNeutralizing;

    private void Start() {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        neutralCircleImage.gameObject.SetActive(false);
    }

    public void SetCapturePointEntity(Entity entity) {
        capturePointEntity = entity;
    }

    private void Update() {
        if (capturePointEntity == Entity.Null) return;

        var capturePoint = entityManager.GetComponentData<CapturePoint>(capturePointEntity);
        
        // Zeige den Neutralisierungskreis wenn wir von einer Kontrolle zu Neutral gehen
        if (capturePoint.controllingFaction != FactionType.None && capturePoint.currentCaptureTime > 0) {
            isNeutralizing = true;
            neutralCircleImage.gameObject.SetActive(true);
            progressCircleImage.gameObject.SetActive(false);
            neutralCircleImage.fillAmount = capturePoint.currentCaptureTime / 5f;
        }
        // Zeige den Einnahmekreis wenn wir von Neutral zu einer Kontrolle gehen
        else if (capturePoint.controllingFaction == FactionType.None && capturePoint.currentCaptureTime > 0) {
            isNeutralizing = false;
            neutralCircleImage.gameObject.SetActive(false);
            progressCircleImage.gameObject.SetActive(true);
            progressCircleImage.fillAmount = capturePoint.currentCaptureTime / 5f;
        }
        // Verstecke beide Kreise wenn keine Einnahme stattfindet
        else {
            neutralCircleImage.gameObject.SetActive(false);
            progressCircleImage.gameObject.SetActive(false);
        }
    }
}