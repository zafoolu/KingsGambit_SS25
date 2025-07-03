using UnityEngine;
using TMPro;

public class UITimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    private float elapsedTime = 0f;

    private void Start()
    {
        if (timerText == null)
        {
            Debug.LogError("Bitte weisen Sie ein TextMeshProUGUI-Komponente für den Timer zu!");
            enabled = false;
            return;
        }
        UpdateTimerDisplay();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}