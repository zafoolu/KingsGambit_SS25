using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerText;
    private float elapsedTime = 0f;
    
    [Header("UI GameObject References")]
    [SerializeField] private GameObject minimapObject;
    [SerializeField] private GameObject unitProductionObject;
    [SerializeField] private GameObject buildingToolObject;
    [SerializeField] private GameObject settingsObject;

    private void Start()
    {
        if (timerText == null)
        {
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
    
    public void ToggleMinimap()
    {
        if (minimapObject != null)
        {
            minimapObject.SetActive(!minimapObject.activeSelf);
        }
    }
    
    public void ToggleUnitProduction()
    {
        if (unitProductionObject != null)
        {
            unitProductionObject.SetActive(!unitProductionObject.activeSelf);
        }
    }
    
    public void ToggleBuildingTool()
    {
        if (buildingToolObject != null)
        {
            buildingToolObject.SetActive(!buildingToolObject.activeSelf);
        }
    }
    
    public void TogglePause()
    {
        Time.timeScale = Time.timeScale == 0f ? 1f : 0f;
    }
    
    public void ToggleSettings()
    {
        if (settingsObject != null)
        {
            settingsObject.SetActive(!settingsObject.activeSelf);
        }
    }
}