using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SkirmishLoader : MonoBehaviour
{
    [SerializeField] private SkirmishSettingsSO skirmishSettings;

    private void Start()
    {
        Debug.Log("SkirmishSceneLoader gestartet");
        StartCoroutine(LoadMapAndApplySettings());
    }

    private IEnumerator LoadMapAndApplySettings()
    {
        // Additiv geladene Map basierend auf der Auswahl
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(skirmishSettings.selectedMapName, LoadSceneMode.Additive);

        // Warte bis vollständig geladen
        yield return new WaitUntil(() => asyncLoad.isDone);

        Debug.Log("Map '" + skirmishSettings.selectedMapName + "' wurde erfolgreich geladen.");

        // Übertrage die Einstellungen
        ApplySettings();
    }

    private void ApplySettings()
    {
        Debug.Log("==== SKIRMISH SETTINGS ====");
        Debug.Log("Map: " + skirmishSettings.selectedMapName);
        Debug.Log("Player Faction: " + skirmishSettings.playerFactionName + " (Index: " + skirmishSettings.playerFactionIndex + ")");
        Debug.Log("AI Faction: " + skirmishSettings.aiFactionName + " (Index: " + skirmishSettings.aiFactionIndex + ")");
        Debug.Log("Victory: DestroyTownhall=" + skirmishSettings.destroyTownhall +
                  ", KillKing=" + skirmishSettings.killKing +
                  ", TimeLimit=" + (skirmishSettings.useTimeLimit ? skirmishSettings.timeLimitMinutes + " Minuten" : "Keine Vorgabe"));

        // Falls du einen GameManager verwendest:
        /*
        GameManager.Instance.SetPlayerFaction(skirmishSettings.playerFactionIndex);
        GameManager.Instance.SetAIFaction(skirmishSettings.aiFactionIndex);
        GameManager.Instance.SetVictoryConditions(
            skirmishSettings.destroyTownhall,
            skirmishSettings.killKing,
            skirmishSettings.timeLimitMinutes
        );
        */

        // Optional: Spielstart triggern
        // GameManager.Instance.StartSkirmish();
    }
}