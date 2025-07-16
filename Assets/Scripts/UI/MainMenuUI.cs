using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button newCampaignButton;
    [SerializeField] private Button battlesButton;
    [SerializeField] private GameObject skirmishMenuUI;

    private void Awake()
    {
        Debug.Log("MainMenuUI Awake");

        // Skirmish-Menü initial ausblenden
        if (skirmishMenuUI != null)
        {
            skirmishMenuUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("SkirmishMenuUI ist nicht zugewiesen!");
        }

        // Kampagnen-Button
        if (newCampaignButton != null)
        {
            newCampaignButton.onClick.AddListener(() =>
            {
                Debug.Log("Campaign clicked");
                SceneManager.LoadScene("CampaignMenu"); // Stelle sicher, dass Szene 1 existiert
            });
        }
        else
        {
            Debug.LogWarning("NewCampaignButton ist nicht zugewiesen!");
        }

        // Battles-Button
        if (battlesButton != null)
        {
            battlesButton.onClick.AddListener(() =>
            {
                Debug.Log("Battles clicked");

                if (skirmishMenuUI != null)
                {
                    bool isActive = skirmishMenuUI.activeSelf;
                    Debug.Log("SkirmishMenuUI is currently " + (isActive ? "active" : "inactive"));
                    skirmishMenuUI.SetActive(!isActive);
                    Debug.Log("SkirmishMenuUI is now " + (!isActive ? "active" : "inactive"));
                }
            });
        }
        else
        {
            Debug.LogWarning("BattlesButton ist nicht zugewiesen!");
        }
    }
}