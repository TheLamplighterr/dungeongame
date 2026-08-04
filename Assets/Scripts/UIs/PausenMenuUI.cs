using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pausePanel;
    public GameObject optionsPanel;

    public CanvasGroup pauseGroup;
    public CanvasGroup optionsGroup;

    [Header("Slide-In Animation")]
    [Tooltip("Das RectTransform des PausePanels (wird automatisch geholt, wenn leer)")]
    [SerializeField] private RectTransform pausePanelRect;
    [SerializeField] private float slideDuration = 0.45f;
    [SerializeField] private float startOffsetY = 1200f; // Startet 1200 Pixel über der Mitte

    private Vector2 originalPosition = Vector2.zero; // Zielposition (Bildschirmmitte: 0,0)
    private Coroutine slideRoutine;

    [Header("Gameplay UI zum Ausblenden")]
    [Tooltip("Ziehe hier die UI-Elemente rein, die beim Pausieren verschwinden und beim Fortsetzen wiederkommen sollen")]
    [SerializeField] private List<GameObject> gameplayUIElementsToHide = new List<GameObject>();
    
    // Merkt sich, welche Gameplay-UIs VOR dem Pausieren wirklich aktiv waren (verhindert Geister-Bossleisten)
    private List<GameObject> previouslyActiveUIElements = new List<GameObject>();

    [Header("Fade")]
    public float fadeSpeed = 6f;

    private bool isPaused = false;
    public bool IsPaused => isPaused;
    private Coroutine fadeRoutine;

    [Header("References")]
    public InventoryUI inventoryUI;

    void Start()
    {
        if (pausePanelRect == null && pausePanel != null)
        {
            pausePanelRect = pausePanel.GetComponent<RectTransform>();
        }

        if (pausePanelRect != null)
        {
            originalPosition = pausePanelRect.anchoredPosition;
        }

        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);

        SetAlpha(pauseGroup, 0);
        SetAlpha(optionsGroup, 0);
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        optionsPanel.SetActive(false); 
        Time.timeScale = 0f;
        isPaused = true;

        ToggleGameplayUI(false);

        PlayerAttack playerAttack = FindFirstObjectByType<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.DisableCombat();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (pauseGroup != null)
        {
            pauseGroup.interactable = true;
            pauseGroup.blocksRaycasts = true;
        }

        StartFade(pauseGroup, 1f);

        // SLIDE-IN VOM OBEREN BILDSCHIRMRAND MIT BOUNCE
        if (pausePanelRect != null)
        {
            StartSlide(new Vector2(originalPosition.x, originalPosition.y + startOffsetY), originalPosition, slideDuration, true);
        }
    }

    public void ResumeGame()
    {
        StartCoroutine(ResumeRoutine());
    }

    IEnumerator ResumeRoutine()
    {
        StartFade(pauseGroup, 0f);

        // SLIDE-OUT NACH OBEN
        if (pausePanelRect != null)
        {
            StartSlide(pausePanelRect.anchoredPosition, new Vector2(originalPosition.x, originalPosition.y + startOffsetY), 0.25f, false);
        }

        ToggleGameplayUI(true);

        PlayerAttack playerAttack = FindFirstObjectByType<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.EnableCombat();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return new WaitForSecondsRealtime(0.25f);

        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    // --- SLIDE & EASE-OUT BOUNCE LOGIK ---

    private void StartSlide(Vector2 startPos, Vector2 targetPos, float duration, bool withBounce)
    {
        if (slideRoutine != null)
            StopCoroutine(slideRoutine);

        slideRoutine = StartCoroutine(SlideRoutine(startPos, targetPos, duration, withBounce));
    }

    private IEnumerator SlideRoutine(Vector2 startPos, Vector2 targetPos, float duration, bool withBounce)
    {
        float elapsed = 0f;
        pausePanelRect.anchoredPosition = startPos;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float curveT = withBounce ? EaseOutBack(t) : Mathf.SmoothStep(0f, 1f, t);

            pausePanelRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, curveT);
            yield return null;
        }

        pausePanelRect.anchoredPosition = targetPos;
    }

    private float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3) + c1 * Mathf.Pow(x - 1f, 2);
    }

    // --- GAMEPLAY UI TOGGLE (SMART) ---

    private void ToggleGameplayUI(bool show)
    {
        if (!show)
        {
            // VOR DEM AUSBLENDEN: Merken, welche UI-Elemente wirklich aktiv waren
            previouslyActiveUIElements.Clear();

            foreach (GameObject uiElement in gameplayUIElementsToHide)
            {
                if (uiElement != null && uiElement.activeSelf)
                {
                    previouslyActiveUIElements.Add(uiElement);
                    uiElement.SetActive(false);
                }
            }
        }
        else
        {
            // BEIM EINBLENDEN: Nur die wieder anmachen, die vorher aktiv waren!
            foreach (GameObject uiElement in previouslyActiveUIElements)
            {
                if (uiElement != null)
                {
                    uiElement.SetActive(true);
                }
            }
            previouslyActiveUIElements.Clear();
        }
    }

    // --- OPTIONS ---

    public void OpenOptions()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);

        if (pauseGroup != null)
        {
            pauseGroup.interactable = false;
            pauseGroup.blocksRaycasts = false;
            SetAlpha(pauseGroup, 0);
        }

        if (optionsGroup != null)
        {
            optionsGroup.interactable = true;
            optionsGroup.blocksRaycasts = true;
        }

        StartFade(optionsGroup, 1f);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);

        if (optionsGroup != null)
        {
            optionsGroup.interactable = false;
            optionsGroup.blocksRaycasts = false;
            SetAlpha(optionsGroup, 0);
        }

        if (pauseGroup != null)
        {
            pauseGroup.interactable = true;
            pauseGroup.blocksRaycasts = true;
        }

        StartFade(pauseGroup, 1f);
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