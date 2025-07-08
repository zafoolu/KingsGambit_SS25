using UnityEngine;
using System.Collections;

public class MinimapJuicyToggle : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform minimapPanel;
    public CanvasGroup canvasGroup; // Optional

    [Header("Animation Settings")]
    public Vector2 hiddenPosition = new Vector2(800, 100);
    public Vector2 visiblePosition = new Vector2(200, 100);
    public float slideDuration = 0.4f;
    public float popScale = 0.9f;
    public float fadeDuration = 0.3f;

    private bool isVisible = true;
    private Coroutine animationCoroutine;

    void Start()
    {
        minimapPanel.anchoredPosition = visiblePosition;
        minimapPanel.localScale = Vector3.one;
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMinimap();
        }
    }

    void ToggleMinimap()
    {
        isVisible = !isVisible;

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimateMinimap(isVisible));
    }

    IEnumerator AnimateMinimap(bool show)
    {
        Vector2 startPos = minimapPanel.anchoredPosition;
        Vector2 targetPos = show ? visiblePosition : hiddenPosition;

        Vector3 startScale = minimapPanel.localScale;
        Vector3 targetScale = show ? Vector3.one : Vector3.one;

        if (show)
        {
            minimapPanel.localScale = Vector3.one * popScale;
        }

        float time = 0f;

        while (time < slideDuration)
        {
            float t = time / slideDuration;
            // Smooth step easing
            t = t * t * (3f - 2f * t);

            minimapPanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

            if (show)
                minimapPanel.localScale = Vector3.Lerp(Vector3.one * popScale, Vector3.one, t);

            if (canvasGroup != null)
            {
                float alphaStart = show ? 0f : 1f;
                float alphaEnd = show ? 1f : 0f;
                canvasGroup.alpha = Mathf.Lerp(alphaStart, alphaEnd, t);
            }

            time += Time.deltaTime;
            yield return null;
        }

        // Final values
        minimapPanel.anchoredPosition = targetPos;
        minimapPanel.localScale = targetScale;
        if (canvasGroup != null)
            canvasGroup.alpha = show ? 1f : 0f;
    }
}