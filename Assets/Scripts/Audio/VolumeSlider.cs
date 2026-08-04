using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    private Slider volumeSlider;

    private void Awake()
    {
        volumeSlider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        // 1. Min/Max Werte erzwingen
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;

        // 2. Den aktuellen Lautstärke-Wert aus dem MusicManager holen
        if (MusicManager.Instance != null)
        {
            // Event kurz entfernen, damit das Setzen des Werts kein Re-Trigger auslöst
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
            volumeSlider.value = MusicManager.Instance.GetVolume();
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    private void OnVolumeChanged(float value)
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetVolume(value);
        }
    }

    private void OnDisable()
    {
        volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
    }
}