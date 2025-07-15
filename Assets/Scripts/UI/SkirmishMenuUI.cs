using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MapSelectionUI : MonoBehaviour
{
    public TMP_Dropdown mapDropdown;               // TextMeshPro-Dropdown
    public TextMeshProUGUI mapNameText;            // Map-Name-Anzeige
    public RawImage mapPreview;                    // Map-Vorschau-Bild

    public Texture2D[] mapPreviewImages;           // Vorschaubilder für Maps

    private string[] maps = { "Carrara", "CursedOnes", "Dreamers" };

    void Start()
    {
        // Dropdown mit Map-Namen füllen
        mapDropdown.ClearOptions();
        mapDropdown.AddOptions(new System.Collections.Generic.List<string>(maps));
        mapDropdown.onValueChanged.AddListener(OnMapChanged);

        // Initial: erste Map anzeigen
        OnMapChanged(0);
    }

    void OnMapChanged(int index)
    {
        // Text aktualisieren
        mapNameText.text = maps[index];

        // Vorschaubild aktualisieren (wenn vorhanden)
        if (mapPreviewImages != null && index < mapPreviewImages.Length && mapPreviewImages[index] != null)
        {
            mapPreview.texture = mapPreviewImages[index];
        }
        else
        {
            mapPreview.texture = null; // Kein Bild → leeren
        }
    }
}