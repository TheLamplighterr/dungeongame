using System.Collections;
using UnityEngine;
using TMPro;

public class ChestInteractionUI : MonoBehaviour
{
    public static ChestInteractionUI Instance { get; private set; }

    [Header("Interaktions-Hinweis (z.B. 'Drücke E zum Öffnen')")]
    [SerializeField] private GameObject interactionPromptGroup;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Item-Benachrichtigung")]
    [SerializeField] private RectTransform notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float notificationDuration = 3.0f;

    [Header("Slide-Einstellungen")]
    [Tooltip("Dauer der Slide-Animation in Sekunden")]
    [SerializeField] private float slideDuration = 0.4f; 
    [Tooltip("Wie weit links außerhalb des Bildschirms gestartet wird")]
    [SerializeField] private float offScreenOffset = -800f; 

    private Vector2 targetPosition;
    private Vector2 offScreenPosition;
    private Coroutine slideCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        HidePrompt();

        if (notificationPanel != null)
        {
            targetPosition = notificationPanel.anchoredPosition;
            offScreenPosition = new Vector2(targetPosition.x + offScreenOffset, targetPosition.y);
            
            notificationPanel.anchoredPosition = offScreenPosition;
            notificationPanel.gameObject.SetActive(false);
        }
    }

    public void ShowPrompt(string message = "Drücke [E] zum Öffnen")
    {
        if (interactionPromptGroup != null)
        {
            if (promptText != null) promptText.text = message;
            interactionPromptGroup.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        if (interactionPromptGroup != null)
        {
            interactionPromptGroup.SetActive(false);
        }
    }

    // Normales Pop-up bei Item-Erhalt
    public void ShowItemNotification(string itemName)
    {
        TriggerNotification($"{itemName} added to Inventory!");
    }

    // Hier war der Fehler: Diese Methode hat in deiner Datei gefehlt!
    public void ShowWarningNotification(string message = "Inventory is full!")
    {
        TriggerNotification($"<color=red>{message}</color>");
    }

    private void TriggerNotification(string textToDisplay)
    {
        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogError("[ChestInteractionUI] 'Notification Panel' oder 'Notification Text' fehlt im Inspector!");
            return;
        }

        notificationText.text = textToDisplay;

        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
        }

        slideCoroutine = StartCoroutine(SlideNotificationRoutine());
    }

    private IEnumerator SlideNotificationRoutine()
    {
        notificationPanel.gameObject.SetActive(true);

        // 1. Reinsliden
        float timer = 0f;
        while (timer < slideDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / slideDuration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            notificationPanel.anchoredPosition = Vector2.Lerp(offScreenPosition, targetPosition, smoothProgress);
            yield return null;
        }
        notificationPanel.anchoredPosition = targetPosition;

        // 2. Warten
        yield return new WaitForSeconds(notificationDuration);

        // 3. Rausliden
        timer = 0f;
        while (timer < slideDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / slideDuration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            notificationPanel.anchoredPosition = Vector2.Lerp(targetPosition, offScreenPosition, smoothProgress);
            yield return null;
        }
        notificationPanel.anchoredPosition = offScreenPosition;
        notificationPanel.gameObject.SetActive(false);
    }
}