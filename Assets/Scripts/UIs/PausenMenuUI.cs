using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic; // Ermöglicht die Nutzung von Listen

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public GameObject optionsPanel;

    public CanvasGroup pauseGroup;
    public CanvasGroup optionsGroup;

    [Header("Gameplay UI zum Ausblenden")]
    [Tooltip("Ziehe hier die UI-Elemente rein, die beim Pausieren verschwinden und beim Fortsetzen wiederkommen sollen")]
    [SerializeField] private List<GameObject> gameplayUIElementsToHide = new List<GameObject>();

    [Header("Fade")]
    public float fadeSpeed = 6f;

    private bool isPaused = false;
    public bool IsPaused => isPaused;
    private Coroutine fadeRoutine;

    [Header("References")]
    public InventoryUI inventoryUI;

    void Start()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);

        SetAlpha(pauseGroup, 0);
        SetAlpha(optionsGroup, 0);
    }

    void Update()
    { 
        
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        optionsPanel.SetActive(false); 
        Time.timeScale = 0f;
        isPaused = true;

        // --- NEU: Gameplay-UI ausblenden ---
        ToggleGameplayUI(false);

        // Kampf deaktivieren & Mauszeiger freigeben
        PlayerAttack playerAttack = FindFirstObjectByType<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.DisableCombat();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // CanvasGroup Interaktion erzwingen
        if (pauseGroup != null)
        {
            pauseGroup.interactable = true;
            pauseGroup.blocksRaycasts = true;
        }

        StartFade(pauseGroup, 1f);
    }

    public void ResumeGame()
    {
        StartCoroutine(ResumeRoutine());
    }

    IEnumerator ResumeRoutine()
    {
        StartFade(pauseGroup, 0f);

        // --- NEU: Gameplay-UI wieder einblenden ---
        ToggleGameplayUI(true);

        // Kampf wieder erlauben & Mauszeiger sperren
        PlayerAttack playerAttack = FindFirstObjectByType<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.EnableCombat();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return new WaitForSecondsRealtime(0.2f);

        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    // Hilfsmethode, um die Gameplay-UI flexibel an- oder auszuschalten
    private void ToggleGameplayUI(bool show)
    {
        foreach (GameObject uiElement in gameplayUIElementsToHide)
        {
            if (uiElement != null)
            {
                uiElement.SetActive(show);
            }
        }
    }

    //  OPTIONS
    public void OpenOptions()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);

        StartFade(optionsGroup, 1f);
        SetAlpha(pauseGroup, 0);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);

        StartFade(pauseGroup, 1f);
        SetAlpha(optionsGroup, 0);
    }

    void StartFade(CanvasGroup group, float target)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(Fade(group, target));
    }

    IEnumerator Fade(CanvasGroup group, float target)
    {
        if (group == null) yield break;

        while (!Mathf.Approximately(group.alpha, target))
        {
            group.alpha = Mathf.Lerp(group.alpha, target, Time.unscaledDeltaTime * fadeSpeed);
            yield return null;
        }

        group.alpha = target;
    }

    void SetAlpha(CanvasGroup group, float value)
    {
        if (group != null)
            group.alpha = value;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menü");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    public void OpenPause()
    {
        PauseGame();
    }

    public void ClosePause()
    {
        ResumeGame();
    }
}