using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // Ermöglicht die Nutzung von Listen

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverPanel;
    public CanvasGroup canvasGroup;
    public RectTransform titleText;
    public CanvasGroup buttonsGroup;

    [Header("Gameplay UI zum Ausblenden")]
    [Tooltip("Ziehe hier alle UI-Elemente rein, die beim GameOver verschwinden sollen (z.B. Action-Icons, Boss-HP-Bar, Standard-HUD)")]
    [SerializeField] private List<GameObject> gameplayUIElementsToHide = new List<GameObject>();

    [Header("Fade Settings")]
    public float fadeDuration = 1.2f;

    [Header("Text Animation")]
    public float textMoveOffset = 60f;
    public float pulseSpeed = 4f;
    public float pulseAmount = 0.05f;

    private Vector2 titleStartPos;

    [Header("Run Stats")]
    public GameObject runStatsPanel;
    public CanvasGroup runStatsGroup;
    public RunStatsUI runStatsUI;

    void Awake()
    {
        gameOverPanel.SetActive(false);

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (runStatsPanel != null)
        {
            runStatsPanel.SetActive(false);
        }

        if (runStatsGroup != null)
        {
            runStatsGroup.alpha = 0f;
            runStatsGroup.interactable = false;
            runStatsGroup.blocksRaycasts = false;
        }

        if (titleText != null)
            titleStartPos = titleText.anchoredPosition;

        if (buttonsGroup != null)
        {
            buttonsGroup.alpha = 0f;
            buttonsGroup.interactable = false;
            buttonsGroup.blocksRaycasts = false;
        }
    }

    public void ShowGameOver()
    {
        // --- NEU: GAMEPLAY-UI AUSBLENDEN ---
        HideGameplayUI();

        // Deaktiviert den Kampf auf dem Spieler, damit Klicks nicht mehr als Angriff gewertet werden
        PlayerAttack playerAttack = FindFirstObjectByType<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.DisableCombat();
        }

        // Schaltet den Mauszeiger wieder frei, damit man die UI-Buttons anklicken kann
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        gameOverPanel.SetActive(true);
        StartCoroutine(FadeIn());
    }

    // Hilfsmethode, um alle registrierten UI-Elemente sauber abzuschalten
    private void HideGameplayUI()
    {
        foreach (GameObject uiElement in gameplayUIElementsToHide)
        {
            if (uiElement != null)
            {
                uiElement.SetActive(false);
            }
        }
    }

    IEnumerator FadeIn()
    {
        float t = 0f;

        Vector2 startPos = titleStartPos + new Vector2(0, textMoveOffset);

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float n = t / fadeDuration;

            // Background Fade
            canvasGroup.alpha = n;

            // Title Slide
            if (titleText != null)
                titleText.anchoredPosition = Vector2.Lerp(startPos, titleStartPos, n);

            // Button Fade später
            if (buttonsGroup != null && n > 0.6f)
            {
                float bn = Mathf.InverseLerp(0.6f, 1f, n);
                buttonsGroup.alpha = bn;
            }

            yield return null;
        }

        canvasGroup.alpha = 1f;

        if (buttonsGroup != null)
        {
            buttonsGroup.alpha = 1f;
            buttonsGroup.interactable = true;
            buttonsGroup.blocksRaycasts = true;
        }

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        StartCoroutine(TitlePulse());
    }

    IEnumerator TitlePulse()
    {
        while (true)
        {
            if (titleText != null)
            {
                float pulse = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
                titleText.localScale = Vector3.one * pulse;
            }

            yield return null;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menü");
    }

    public void OpenRunStats()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (runStatsPanel != null)
        {
            runStatsPanel.SetActive(true);
        }

        if (runStatsUI != null)
        {
            runStatsUI.UpdateStats(); 
        }

        if (runStatsGroup != null)
        {
            runStatsGroup.alpha = 1f;
            runStatsGroup.interactable = true;
            runStatsGroup.blocksRaycasts = true;
        }
    }

    public void CloseRunStats()
    {
        if (runStatsPanel != null)
        {
            runStatsPanel.SetActive(false);
        }

        if (runStatsGroup != null)
        {
            runStatsGroup.alpha = 0f;
            runStatsGroup.interactable = false;
            runStatsGroup.blocksRaycasts = false;
        }

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
}