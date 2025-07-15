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

    [Header("Faction Selection")]
    public TMP_Dropdown factionDropdown;
    public RawImage factionPreviewImage;
    public Texture2D[] factionPreviewImages;
    public string[] factionNames;

    private int currentFactionIndex = 0;

    void Start()
    {
        // --- Map Dropdown ---
        mapDropdown.ClearOptions();
        mapDropdown.AddOptions(new List<string>(maps));
        mapDropdown.onValueChanged.AddListener(OnMapChanged);
        OnMapChanged(0);

        // --- Time Limit Dropdown ---
        timeLimitDropdown.ClearOptions();
        timeLimitDropdown.AddOptions(new List<string> {
            "Keine Vorgabe", "5 Minuten", "10 Minuten", "30 Minuten", "60 Minuten"
        });

        timeLimitToggle.onValueChanged.AddListener((value) =>
        {
            timeLimitDropdown.gameObject.SetActive(value);
        });

        timeLimitDropdown.gameObject.SetActive(timeLimitToggle.isOn);

        // --- Faction Dropdown ---
        if (factionNames != null && factionDropdown != null)
        {
            factionDropdown.ClearOptions();
            factionDropdown.AddOptions(new List<string>(factionNames));
            factionDropdown.onValueChanged.AddListener(OnFactionChanged);
            OnFactionChanged(0); // initial setzen
        }
    }

    void OnMapChanged(int index)
    {
        mapNameText.text = maps[index];

        if (mapPreviewImages != null && index < mapPreviewImages.Length && mapPreviewImages[index] != null)
        {
            mapPreview.texture = mapPreviewImages[index];
        }
        else
        {
            mapPreview.texture = null;
        }
    }

    void OnFactionChanged(int index)
    {
        currentFactionIndex = index;

        if (factionPreviewImages != null && index < factionPreviewImages.Length && factionPreviewImages[index] != null)
        {
            factionPreviewImage.texture = factionPreviewImages[index];
        }
        else
        {
            factionPreviewImage.texture = null;
        }
    }

    // Zugriff auf ausgewählte Siegbedingungen
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

    // Zugriff auf gewählte Fraktion
    public FactionSelection GetSelectedFaction()
    {
        return new FactionSelection
        {
            factionIndex = currentFactionIndex,
            factionName = factionNames[currentFactionIndex]
        };
    }

    // Structs
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