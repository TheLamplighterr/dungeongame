using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Slider))]
public class SFXVolumeSlider : MonoBehaviour, IDragHandler
{
    private Slider volumeSlider;
    [SerializeField] private string testSoundName = "Slider";
    
    [Header("Feedback Sound Timing")]
    [SerializeField] private float soundCooldown = 0.15f; // Zeit zwischen den Sounds beim Ziehen
    private float lastSoundTime;

    private void Awake()
    {
        volumeSlider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;

        if (SFXManager.Instance != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
            volumeSlider.value = SFXManager.Instance.GetVolume();
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    private void OnVolumeChanged(float value)
    {
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.SetVolume(value);
        }
    }

    // Wird automatisch von Unity aufgerufen, SO LANGE du den Slider ziehst
    public void OnDrag(PointerEventData eventData)
    {
        if (Time.unscaledTime - lastSoundTime >= soundCooldown)
        {
            PlayTestSound();
            lastSoundTime = Time.unscaledTime;
        }
    }

    private void PlayTestSound()
    {
        if (SFXManager.Instance != null && !string.IsNullOrEmpty(testSoundName))
        {
            SFXManager.Instance.PlaySFX(testSoundName);
        }
    }

    private void OnDisable()
    {
        volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
    }
}