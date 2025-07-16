using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private SkirmishSettingsSO skirmishSettings;

    private float remainingTime;
    private bool countdownActive = false;
    private bool timerAvailable = true; // <-- NEU

    [Header("UI GameObject References")]
    [SerializeField] private GameObject minimapObject;
    [SerializeField] private GameObject unitProductionObject;
    [SerializeField] private GameObject buildingToolObject;
    [SerializeField] private GameObject settingsObject;

    private void Start()
    {
        if (timerText == null || skirmishSettings == null)
        {
            Debug.LogWarning("⚠️ TimerText oder SkirmishSettingsSO fehlt! Timer wird deaktiviert.");
            timerAvailable = false;
            return; // Alles andere weiter normal
        }

        if (skirmishSettings.useTimeLimit)
        {
            remainingTime = skirmishSettings.timeLimitMinutes * 60f;
            countdownActive = true;
        }
        else
        {
            countdownActive = false;
            timerText.gameObject.SetActive(false);
        }

        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (!countdownActive || !timerAvailable) return;

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
        if (!timerAvailable || timerText == null) return;

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnTimeLimitReached()
    {
        Debug.Log("🛑 Zeitlimit erreicht – Game Over!");
        // z. B. UI anzeigen oder Spiel beenden
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