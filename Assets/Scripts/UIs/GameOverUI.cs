using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverPanel;
    public CanvasGroup canvasGroup;
    public RectTransform titleText;
    public CanvasGroup buttonsGroup;

    [Header("Fade Settings")]
    public float fadeDuration = 1.2f;

    [Header("Text Animation")]
    public float textMoveOffset = 60f;
    public float pulseSpeed = 4f;
    public float pulseAmount = 0.05f;

    private Vector2 titleStartPos;

    void Awake()
    {
        gameOverPanel.SetActive(false);

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

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
        gameOverPanel.SetActive(true);
        StartCoroutine(FadeIn());
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
}