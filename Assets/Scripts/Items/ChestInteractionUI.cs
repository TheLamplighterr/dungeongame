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
    [SerializeField] private RectTransform notificationPanel; // Das Pergament-Panel
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float notificationDuration = 3.0f; // Wie lange es stehen bleibt

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
            // Richtige Zielposition auf dem Canvas merken
            targetPosition = notificationPanel.anchoredPosition;
            offScreenPosition = new Vector2(targetPosition.x + offScreenOffset, targetPosition.y);
            
            // Zu Beginn nach links aus dem Bild schieben und deaktivieren
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

    public void ShowItemNotification(string itemName)
    {
        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogError("[ChestInteractionUI] 'Notification Panel' oder 'Notification Text' fehlt im Inspector!");
            return;
        }

        notificationText.text = $"{itemName} added to Inventory!";

        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
        }

        slideCoroutine = StartCoroutine(SlideNotificationRoutine());
    }

    private IEnumerator SlideNotificationRoutine()
    {
        notificationPanel.gameObject.SetActive(true);

        // --- 1. REINSLIDEN (von links nach rechts) ---
        float timer = 0f;
        while (timer < slideDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / slideDuration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress); // Macht die Bewegung geschmeidig

            notificationPanel.anchoredPosition = Vector2.Lerp(offScreenPosition, targetPosition, smoothProgress);
            yield return null;
        }
        notificationPanel.anchoredPosition = targetPosition;

        // --- 2. ANZEIGEDAUER ---
        yield return new WaitForSeconds(notificationDuration);

        // --- 3. RAUSSLIDEN (wieder nach links) ---
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