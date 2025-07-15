using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapSelectionUI : MonoBehaviour
{
    [Header("Map Selection")]
    public TMP_Dropdown mapDropdown;               // TextMeshPro-Dropdown
    public TextMeshProUGUI mapNameText;            // Map-Name-Anzeige
    public RawImage mapPreview;                    // Map-Vorschau-Bild
    public Texture2D[] mapPreviewImages;           // Vorschaubilder für Maps

    private string[] maps = { "Carrara", "CursedOnes", "Dreamers" };

    [Header("Victory Conditions")]
    public Toggle destroyTownhallToggle;
    public Toggle killKingToggle;
    public Toggle timeLimitToggle;
    public TMP_Dropdown timeLimitDropdown;

    // Minuten-Werte passend zur Dropdown-Reihenfolge
    private readonly int[] timeLimitOptions = { 0, 5, 10, 30, 60 };

    void Start()
    {
        // Map Dropdown initialisieren
        mapDropdown.ClearOptions();
        mapDropdown.AddOptions(new List<string>(maps));
        mapDropdown.onValueChanged.AddListener(OnMapChanged);
        OnMapChanged(0); // Initiale Map anzeigen

        // Time Limit Dropdown initialisieren
        timeLimitDropdown.ClearOptions();
        timeLimitDropdown.AddOptions(new List<string> {
            "Keine Vorgabe", "5 Minuten", "10 Minuten", "30 Minuten", "60 Minuten"
        });

        // Toggle → Dropdown anzeigen/verstecken
        timeLimitToggle.onValueChanged.AddListener((value) =>
        {
            timeLimitDropdown.gameObject.SetActive(value);
        });

        timeLimitDropdown.gameObject.SetActive(timeLimitToggle.isOn); // initialer Zustand
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

    // Zugriff auf ausgewählte Siegbedingungen
    public VictoryConditionSelection GetSelectedVictoryConditions()
    {
        VictoryConditionSelection selection = new VictoryConditionSelection
        {
            destroyTownhall = destroyTownhallToggle.isOn,
            killKing = killKingToggle.isOn,
            timeLimitEnabled = timeLimitToggle.isOn,
            timeLimitMinutes = timeLimitToggle.isOn ? timeLimitOptions[timeLimitDropdown.value] : 0
        };

        return selection;
    }

    // Datenstruktur für Siegbedingungen
    public struct VictoryConditionSelection
    {
        public bool destroyTownhall;
        public bool killKing;
        public bool timeLimitEnabled;
        public int timeLimitMinutes;
    }
}