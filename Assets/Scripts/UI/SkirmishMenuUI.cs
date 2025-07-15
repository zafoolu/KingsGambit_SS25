using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapSelectionUI : MonoBehaviour
{
    [Header("Map Selection")]
    public TMP_Dropdown mapDropdown;
    public TextMeshProUGUI mapNameText;
    public RawImage mapPreview;
    public Texture2D[] mapPreviewImages;
    private string[] maps = { "Carrara", "CursedOnes", "Dreamers" };

    [Header("Victory Conditions")]
    public Toggle destroyTownhallToggle;
    public Toggle killKingToggle;
    public Toggle timeLimitToggle;
    public TMP_Dropdown timeLimitDropdown;
    private readonly int[] timeLimitOptions = { 0, 5, 10, 30, 60 };

    [Header("Player Faction Selection")]
    public TMP_Dropdown playerFactionDropdown;
    public RawImage playerFactionPreview;
    public Texture2D[] playerFactionImages;
    public string[] playerFactionNames;
    private int currentPlayerFactionIndex = 0;

    [Header("AI Faction Selection")]
    public TMP_Dropdown aiFactionDropdown;
    public RawImage aiFactionPreview;
    public Texture2D[] aiFactionImages;
    public string[] aiFactionNames;
    private int currentAIFactionIndex = 0;

    void Start()
    {
        // Map Auswahl
        mapDropdown.ClearOptions();
        mapDropdown.AddOptions(new List<string>(maps));
        mapDropdown.onValueChanged.AddListener(OnMapChanged);
        OnMapChanged(0);

        // Time Limit Dropdown
        timeLimitDropdown.ClearOptions();
        timeLimitDropdown.AddOptions(new List<string> {
            "Keine Vorgabe", "5 Minuten", "10 Minuten", "30 Minuten", "60 Minuten"
        });
        timeLimitToggle.onValueChanged.AddListener((value) =>
        {
            timeLimitDropdown.gameObject.SetActive(value);
        });
        timeLimitDropdown.gameObject.SetActive(timeLimitToggle.isOn);

        // Player Faction
        playerFactionDropdown.ClearOptions();
        playerFactionDropdown.AddOptions(new List<string>(playerFactionNames));
        playerFactionDropdown.onValueChanged.AddListener(OnPlayerFactionChanged);
        OnPlayerFactionChanged(0);

        // AI Faction
        aiFactionDropdown.ClearOptions();
        aiFactionDropdown.AddOptions(new List<string>(aiFactionNames));
        aiFactionDropdown.onValueChanged.AddListener(OnAIFactionChanged);
        OnAIFactionChanged(0);
    }

    void OnMapChanged(int index)
    {
        mapNameText.text = maps[index];

        if (mapPreviewImages != null && index < mapPreviewImages.Length && mapPreviewImages[index] != null)
            mapPreview.texture = mapPreviewImages[index];
        else
            mapPreview.texture = null;
    }

    void OnPlayerFactionChanged(int index)
    {
        currentPlayerFactionIndex = index;
        if (playerFactionImages != null && index < playerFactionImages.Length && playerFactionImages[index] != null)
            playerFactionPreview.texture = playerFactionImages[index];
        else
            playerFactionPreview.texture = null;
    }

    void OnAIFactionChanged(int index)
    {
        currentAIFactionIndex = index;
        if (aiFactionImages != null && index < aiFactionImages.Length && aiFactionImages[index] != null)
            aiFactionPreview.texture = aiFactionImages[index];
        else
            aiFactionPreview.texture = null;
    }

    public VictoryConditionSelection GetSelectedVictoryConditions()
    {
        return new VictoryConditionSelection
        {
            destroyTownhall = destroyTownhallToggle.isOn,
            killKing = killKingToggle.isOn,
            timeLimitEnabled = timeLimitToggle.isOn,
            timeLimitMinutes = timeLimitToggle.isOn ? timeLimitOptions[timeLimitDropdown.value] : 0
        };
    }

    public FactionSelection GetPlayerFaction()
    {
        return new FactionSelection
        {
            factionIndex = currentPlayerFactionIndex,
            factionName = playerFactionNames[currentPlayerFactionIndex]
        };
    }

    public FactionSelection GetAIFaction()
    {
        return new FactionSelection
        {
            factionIndex = currentAIFactionIndex,
            factionName = aiFactionNames[currentAIFactionIndex]
        };
    }

    public struct VictoryConditionSelection
    {
        public bool destroyTownhall;
        public bool killKing;
        public bool timeLimitEnabled;
        public int timeLimitMinutes;
    }

    public struct FactionSelection
    {
        public int factionIndex;
        public string factionName;
    }
}