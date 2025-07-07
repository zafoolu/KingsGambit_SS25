using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MinimapClickHandler : MonoBehaviour
{
    [Header("References")]
    public Camera minimapCamera; // Orthographic top-down camera
    public Transform cinemachineFollowTarget; // The object your Cinemachine camera follows
    public RectTransform minimapRectTransform; // RawImage RectTransform
    public InputActionReference clickAction; // Your UI Click input action

    [Header("Feinjustierung")]
    public Vector2 worldOffset = new Vector2(50f, 50f); // Optional offset to fix alignment manually

    private void OnEnable()
    {
        clickAction.action.performed += OnClickPerformed;
        clickAction.action.Enable();
    }

    private void OnDisable()
    {
        clickAction.action.performed -= OnClickPerformed;
        clickAction.action.Disable();
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Ist der Klick innerhalb der Minimap?
        if (!RectTransformUtility.RectangleContainsScreenPoint(minimapRectTransform, mousePos))
            return;

        // In lokalen Koordinaten der Minimap umrechnen
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRectTransform,
            mousePos,
            null,
            out Vector2 localPoint
        );

        // Normalisieren in Bereich [0, 1]
        Vector2 normalized = new Vector2(
            (localPoint.x / minimapRectTransform.rect.width) + 0.5f,
            (localPoint.y / minimapRectTransform.rect.height) + 0.5f
        );

        // Sichtbare Fläche der Kamera in Weltkoordinaten
        float height = minimapCamera.orthographicSize * 2f;
        float width = height * minimapCamera.aspect;

        // Mittelpunkt der Kamera (zentriert auf Map)
        Vector3 center = minimapCamera.transform.position;

        // Zielkoordinaten berechnen + Offset draufrechnen
        float worldX = (normalized.x - 0.5f) * width + center.x + worldOffset.x;
        float worldZ = (normalized.y - 0.5f) * height + center.z + worldOffset.y;

        // Neue Position setzen (Höhe bleibt gleich)
        Vector3 target = new Vector3(worldX, cinemachineFollowTarget.position.y, worldZ);
        cinemachineFollowTarget.position = target;

        Debug.Log($"[Minimap] Klick bei {mousePos} → Weltziel: {target}");
    }
}