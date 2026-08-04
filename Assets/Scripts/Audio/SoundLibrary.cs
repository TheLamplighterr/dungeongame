using UnityEngine;

[System.Serializable]
public struct SoundEffect
{
    public string soundName; // z.B. "ButtonClick", "SwordSwing", "PlayerHit"
    public AudioClip clip;
}

public class SoundLibrary : MonoBehaviour
{
    public SoundEffect[] soundEffects;

    public AudioClip GetClipFromName(string soundName)
    {
        if (soundEffects == null || soundEffects.Length == 0)
        {
            Debug.LogError("❌ SoundLibrary: Es sind KEINE Soundeffekte eingetragen!");
            return null;
        }

        foreach (var effect in soundEffects)
        {
            if (effect.soundName == soundName)
            {
                return effect.clip;
            }
        }

        Debug.LogWarning($"⚠️ Sound-Effect '{soundName}' wurde in der SoundLibrary nicht gefunden.");
        return null;
    }
}