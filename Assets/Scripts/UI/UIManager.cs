using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private SkirmishSettingsSO skirmishSettings;

    private float remainingTime;
    private bool countdownActive = false;

    [Header("UI GameObject References")]
    [SerializeField] private GameObject minimapObject;
    [SerializeField] private GameObject unitProductionObject;
    [SerializeField] private GameObject buildingToolObject;
    [SerializeField] private GameObject settingsObject;

    private void Start()
    {
        if (timerText == null || skirmishSettings == null)
        {
            enabled = false;
            Debug.LogError("❌ TimerText oder SkirmishSettingsSO fehlt!");
            return;
        }

        if (skirmishSettings.useTimeLimit)
        {
            remainingTime = skirmishSettings.timeLimitMinutes * 60f;
            countdownActive = true;
        }
        else
        {
            countdownActive = false;
            timerText.gameObject.SetActive(false); // Timer verstecken, wenn nicht aktiv
        }

        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (!countdownActive) return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            countdownActive = false;
            OnTimeLimitReached();
        }

        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnTimeLimitReached()
    {
        Debug.Log("🛑 Zeitlimit erreicht – Game Over!");
        // Hier kannst du z. B. eine GameOver-UI aktivieren oder Szene wechseln:
        // Time.timeScale = 0f;
        // SceneManager.LoadScene("GameOverScene");
    }

    public void ToggleMinimap()
    {
        if (minimapObject != null)
            minimapObject.SetActive(!minimapObject.activeSelf);
    }

    public void ToggleUnitProduction()
    {
        if (unitProductionObject != null)
            unitProductionObject.SetActive(!unitProductionObject.activeSelf);
    }

    public void ToggleBuildingTool()
    {
        if (buildingToolObject != null)
            buildingToolObject.SetActive(!buildingToolObject.activeSelf);
    }

    public void TogglePause()
    {
        Time.timeScale = Time.timeScale == 0f ? 1f : 0f;
    }

    public void ToggleSettings()
    {
        if (settingsObject != null)
            settingsObject.SetActive(!settingsObject.activeSelf);
    }
}