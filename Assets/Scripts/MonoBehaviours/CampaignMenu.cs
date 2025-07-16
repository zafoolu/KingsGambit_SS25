using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FactionSelectUI : MonoBehaviour
{
    [SerializeField] private Button carraraButton;
    [SerializeField] private Button cursedButton;
    [SerializeField] private Button backButton;

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
                SceneManager.LoadScene("MainMenuScene"); // oder Index, z.B. SceneManager.LoadScene(0);
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
