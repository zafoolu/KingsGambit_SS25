using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FactionSelectUI : MonoBehaviour
{
    [SerializeField] private Button carraraButton;
    [SerializeField] private Button cursedButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button startCampaignButton; // <-- NEU

    [SerializeField] private GameObject carraraCanvas;
    [SerializeField] private GameObject cursedCanvas;

    private void Start()
    {
        // Alle Fraktions-UI deaktivieren
        if (carraraCanvas != null) carraraCanvas.SetActive(false);
        if (cursedCanvas != null) cursedCanvas.SetActive(false);

        // Fraktionsauswahl
        if (carraraButton != null)
        {
            carraraButton.onClick.AddListener(() =>
            {
                ShowFactionCanvas(carraraCanvas);
            });
        }

        if (cursedButton != null)
        {
            cursedButton.onClick.AddListener(() =>
            {
                ShowFactionCanvas(cursedCanvas);
            });
        }

        // Zurück-Button
        if (backButton != null)
        {
            backButton.onClick.AddListener(() =>
            {
                Debug.Log("Zurück zum Hauptmenü");
                SceneManager.LoadScene("MainMenuScene"); // oder SceneManager.LoadScene(0);
            });
        }

        // Start Campaign Button
        if (startCampaignButton != null)
        {
            startCampaignButton.onClick.AddListener(() =>
            {
                Debug.Log("Start Campaign");
                SceneManager.LoadScene("Campaign"); // Szene muss in Build Settings gelistet sein
            });
        }
    }

    private void ShowFactionCanvas(GameObject selectedCanvas)
    {
        if (carraraCanvas != null) carraraCanvas.SetActive(false);
        if (cursedCanvas != null) cursedCanvas.SetActive(false);

        if (selectedCanvas != null) selectedCanvas.SetActive(true);
    }
}