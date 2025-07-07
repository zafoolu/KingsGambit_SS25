using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CameraFrustumLine : MonoBehaviour
{
    public Camera targetCamera; // Deine Hauptkamera
    public float groundY = 0f;  // Ebene (z. B. Y=0)

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 4;
        lineRenderer.loop = true;
    }

    void LateUpdate()
    {
        Vector3[] corners = new Vector3[4];
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, groundY, 0));

        for (int i = 0; i < 4; i++)
        {
            Vector2 vp = i switch
            {
                0 => new Vector2(0, 0), // Bottom Left
                1 => new Vector2(0, 1), // Top Left
                2 => new Vector2(1, 1), // Top Right
                3 => new Vector2(1, 0), // Bottom Right
                _ => Vector2.zero
            };

            Ray ray = targetCamera.ViewportPointToRay(vp);
            if (groundPlane.Raycast(ray, out float distance))
            {
                corners[i] = ray.GetPoint(distance) + Vector3.up * 0.01f; // leicht über Boden
            }
        }

        lineRenderer.SetPositions(corners);
    }
}
