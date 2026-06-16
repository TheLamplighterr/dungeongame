using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public GameObject optionsPanel;

    public CanvasGroup pauseGroup;
    public CanvasGroup optionsGroup;

    [Header("Fade")]
    public float fadeSpeed = 6f;

    private bool isPaused = false;
    private Coroutine fadeRoutine;

    void Start()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);

        SetAlpha(pauseGroup, 0);
        SetAlpha(optionsGroup, 0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel.activeSelf)
            {
                CloseOptions();
                return;
            }

            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        StartFade(pauseGroup, 1f);
    }

    public void ResumeGame()
    {
        StartCoroutine(ResumeRoutine());
    }

    IEnumerator ResumeRoutine()
    {
        StartFade(pauseGroup, 0f);

        yield return new WaitForSecondsRealtime(0.2f);

        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
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
}