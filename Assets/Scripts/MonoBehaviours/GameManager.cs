using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string selectedMap;
    public string selectedFaction;
    public float timeLimit = 300f; // 5 Minuten
    private float timer;
    public bool gameRunning = false;

    public enum GameGoal { KillKing, DestroyTownHall }
    public GameGoal currentGoal;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Update()
    {
        if (!gameRunning) return;

        timer += Time.deltaTime;
        if (timer >= timeLimit)
        {
            EndGame(false, "Zeit abgelaufen");
        }
    }

    public void StartGame()
    {
        timer = 0f;
        gameRunning = true;
        SceneManager.LoadScene(selectedMap);
    }

    public void EndGame(bool playerWon, string reason)
    {
        gameRunning = false;
        Debug.Log("Spiel beendet: " + (playerWon ? "Gewonnen" : "Verloren") + ", Grund: " + reason);
        // Hier kannst du UI für Sieg/Niederlage anzeigen
    }

    // Beispielmethode, um Siegbedingungen zu prüfen
    public void CheckGoalStatus()
    {
        // TODO: Implementiere Logik, z.B. ob König oder Town Hall zerstört wurde
    }
}