using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SkirmishLoader : MonoBehaviour
{
    [SerializeField] private SkirmishSettingsSO skirmishSettings;

    private void Start()
    {
        Debug.Log("📦 SkirmishSceneLoader gestartet.");
        StartCoroutine(LoadMapAndLogSettings());
    }

    private IEnumerator LoadMapAndLogSettings()
    {
        Debug.Log("🌍 Lade Map: " + skirmishSettings.selectedMapName);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(skirmishSettings.selectedMapName, LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncLoad.isDone);

        Debug.Log("✅ Map wurde erfolgreich geladen.");
        LogSettings();
    }

    private void LogSettings()
    {
        Debug.Log("==== SKIRMISH SETTINGS ====");
        Debug.Log("🗺️ Map: " + skirmishSettings.selectedMapName);
        Debug.Log("🧍 Player Faction: " + skirmishSettings.playerFactionName + " (Index: " + skirmishSettings.playerFactionIndex + ")");
        Debug.Log("🤖 AI Faction: " + skirmishSettings.aiFactionName + " (Index: " + skirmishSettings.aiFactionIndex + ")");
        Debug.Log("🎯 Victory Conditions:");
        Debug.Log(" - Destroy Townhall: " + skirmishSettings.destroyTownhall);
        Debug.Log(" - Kill King: " + skirmishSettings.killKing);
        Debug.Log(" - Time Limit: " + (skirmishSettings.useTimeLimit ? skirmishSettings.timeLimitMinutes + " Minuten" : "Keine Vorgabe"));
    }
}