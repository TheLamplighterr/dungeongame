using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class UIButtonSoundAndTween : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Sound Names (SoundLibrary)")]
    [SerializeField] private string hoverSoundName = "ButtonHover";
    [SerializeField] private string clickSoundName = "ButtonClick";

    [Header("Scaling Movement")]
    [SerializeField] private float scaleMultiplier = 1.05f; // Wie viel größer? (1.0 = normal)
    [SerializeField] private float tweenSpeed = 15f; // Wie schnell skaliert er?

    private Button button;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovering = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale; // Merkt sich die Startgröße
        targetScale = originalScale;
    }

    private void OnDisable()
    {
        // Falls der Button deaktiviert wird, während er skaliert ist, stellen wir ihn zurück.
        transform.localScale = originalScale;
        targetScale = originalScale;
        isHovering = false;
    }

    // --- SOUND & MOVEMENT TRIGGER ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            // 1. Sound abspielen mit Pitch (wir nutzen die letzte Methode!)
            if (SFXManager.Instance != null && !string.IsNullOrEmpty(hoverSoundName))
            {
                SFXManager.Instance.PlaySFXWithPitch(hoverSoundName, 0.95f, 1.05f);
            }

            // 2. Bewegungs-Ziel setzen
            targetScale = originalScale * scaleMultiplier;
            isHovering = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button != null) // Exit immer erlauben, auch wenn nicht interactable
        {
            // Bewegungs-Ziel wieder zurücksetzen
            targetScale = originalScale;
            isHovering = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            // 1. Sound abspielen
            if (SFXManager.Instance != null && !string.IsNullOrEmpty(clickSoundName))
            {
                SFXManager.Instance.PlaySFX(clickSoundName);
            }

            // Optional: Beim Klick kurz auf Normalgröße zuschnappen lassen als Feedback
            transform.localScale = originalScale;
        }
    }

    // --- SMOOTH MOVEMENT UPDATE ---

    private void Update()
    {
        // Wenn die Zielgröße noch nicht erreicht ist, bewegen wir uns glatt dorthin.
        if (transform.localScale != targetScale)
        {
            // Time.unscaledDeltaTime nutzen wir, damit es auch im Pausenmenü (Dungeon) flüssig aussieht!
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * tweenSpeed);
        }
    }
}