using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TextBubbleUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject bubblePanel;
    [SerializeField] private TextMeshProUGUI bubbleText;
    [SerializeField] private GameObject waveCountdownPanel;
    [SerializeField] private TextMeshProUGUI waveCountdownText;
    
    [Header("Text Content")]
    [SerializeField] private List<string> textMessages = new List<string>
    {
        "Das ist die erste Text-Bubble! Klicke irgendwo für die nächste Nachricht.",
        "Hier ist die zweite Nachricht! Das Spiel ist pausiert, während du das liest."
    };
    
    [Header("Wave Countdown Settings")]
    [SerializeField] private float waveCountdownTime = 75f; // 75 Sekunden
    [SerializeField] private bool showWaveCountdown = true;
    
    [Header("Settings")]
    [SerializeField] private bool pauseGameWhenActive = true;
    
    private int currentMessageIndex = 0;
    private bool isActive = false;
    private float originalTimeScale;
    private bool hasBeenUsed = false;
    
    // Wave Countdown Variablen
    private float currentWaveCountdown;
    private int waveNumber = 1;
    private bool waveCountdownActive = false;
    
    void Start()
    {
        // Initialisierung
        originalTimeScale = Time.timeScale;
        currentWaveCountdown = waveCountdownTime;
        
        // Erste Nachricht setzen
        if (textMessages.Count > 0 && bubbleText != null)
            bubbleText.text = textMessages[0];
        
        // Wave Countdown Panel initial verstecken
        if (waveCountdownPanel != null)
            waveCountdownPanel.SetActive(false);
        
        // Sofort beim Start aktivieren
        ShowBubble();
    }
    
    void Update()
    {
        // Nur auf Klicks reagieren wenn aktiv
        if (isActive && Input.GetMouseButtonDown(0))
        {
            NextMessage();
        }
        
        // Wave Countdown Update (läuft immer, auch wenn Bubble aktiv ist)
        if (waveCountdownActive && showWaveCountdown)
        {
            // Countdown nur reduzieren wenn das Spiel nicht pausiert ist
            if (Time.timeScale > 0f)
            {
                currentWaveCountdown -= Time.deltaTime;
            }
            
            UpdateWaveCountdownDisplay();
            
            if (currentWaveCountdown <= 0f)
            {
                OnWaveSpawn();
                currentWaveCountdown = waveCountdownTime; // Reset auf 75 Sekunden
                waveNumber++;
            }
        }
    }
    
    public void ShowBubble()
    {
        if (bubblePanel == null || hasBeenUsed) return;
        
        isActive = true;
        currentMessageIndex = 0;
        
        // Panel anzeigen
        bubblePanel.SetActive(true);
        
        // Spiel pausieren
        if (pauseGameWhenActive)
        {
            originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        
        // Erste Nachricht anzeigen
        UpdateBubbleText();
        
        Debug.Log("Text Bubble aktiviert - Spiel pausiert");
    }
    
    public void NextMessage()
    {
        currentMessageIndex++;
        
        // Prüfen ob noch Nachrichten vorhanden sind
        if (currentMessageIndex >= textMessages.Count)
        {
            // Alle Nachrichten durchlaufen - Bubble schließen und Wave Countdown starten
            CloseBubble();
            StartWaveCountdown();
            return;
        }
        
        // Nächste Nachricht anzeigen
        UpdateBubbleText();
        
        Debug.Log($"Nächste Nachricht: {currentMessageIndex + 1}/{textMessages.Count}");
    }
    
    public void CloseBubble()
    {
        if (bubblePanel == null) return;
        
        isActive = false;
        hasBeenUsed = true;
        
        // Panel verstecken
        bubblePanel.SetActive(false);
        
        // Spiel fortsetzen
        if (pauseGameWhenActive)
        {
            Time.timeScale = originalTimeScale;
        }
        
        Debug.Log("Text Bubble geschlossen - Spiel fortgesetzt");
    }
    
    public void StartWaveCountdown()
    {
        waveCountdownActive = true;
        currentWaveCountdown = waveCountdownTime; // Starte mit 75 Sekunden
        
        if (waveCountdownPanel != null && showWaveCountdown)
        {
            waveCountdownPanel.SetActive(true);
            UpdateWaveCountdownDisplay();
        }
        
        Debug.Log("Wave Countdown gestartet - 75 Sekunden bis zur nächsten Wave");
    }
    
    private void UpdateWaveCountdownDisplay()
    {
        if (waveCountdownText == null) return;
        
        // Countdown in Sekunden anzeigen (gerundet)
        int seconds = Mathf.CeilToInt(currentWaveCountdown);
        
        // Text formatieren
        waveCountdownText.text = $"Next Wave {waveNumber} in: {seconds}";
    }
    
    private void OnWaveSpawn()
    {
        Debug.Log($"Wave {waveNumber} gespawnt! Countdown startet neu mit 75 Sekunden.");
        // Hier könntest du ein Event für das SimpleSpawnerSystem auslösen
    }
    
    private void UpdateBubbleText()
    {
        if (bubbleText == null || textMessages.Count == 0) return;
        
        // Text aktualisieren
        if (currentMessageIndex < textMessages.Count)
        {
            bubbleText.text = textMessages[currentMessageIndex];
        }
    }
    
    // Public Methoden für externe Nutzung
    public void AddMessage(string message)
    {
        textMessages.Add(message);
    }
    
    public void ClearMessages()
    {
        textMessages.Clear();
        currentMessageIndex = 0;
    }
    
    public void SetMessages(List<string> newMessages)
    {
        textMessages = newMessages;
        currentMessageIndex = 0;
    }
    
    public void ToggleWaveCountdown(bool show)
    {
        showWaveCountdown = show;
        if (waveCountdownPanel != null)
            waveCountdownPanel.SetActive(show && waveCountdownActive);
    }
    
    public float GetRemainingWaveTime()
    {
        return currentWaveCountdown;
    }
    
    public int GetCurrentWave()
    {
        return waveNumber;
    }
    
    public bool IsWaveCountdownActive => waveCountdownActive;
    
    public bool IsActive => isActive;
    
    void OnDestroy()
    {
        // Sicherstellen, dass das Spiel nicht pausiert bleibt
        if (isActive && pauseGameWhenActive)
        {
            Time.timeScale = originalTimeScale;
        }
    }
}