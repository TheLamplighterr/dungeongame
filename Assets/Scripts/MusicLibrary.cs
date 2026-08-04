using UnityEngine;

[System.Serializable]
public struct MusicTrack
{
    public string trackName;
    public AudioClip clip;
}

public class MusicLibrary : MonoBehaviour
{
    public MusicTrack[] tracks;

    public AudioClip GetClipFromName(string trackName)
    {
        // Schutz vor leeren Arrays/Uninitialisiertem Zustand
        if (tracks == null || tracks.Length == 0)
        {
            Debug.LogError("❌ MusicLibrary: Es sind KEINE Tracks in der Liste eingetragen!");
            return null;
        }

        foreach (var track in tracks)
        {
            if (track.trackName == trackName)
            {
                return track.clip;
            }
        }

        Debug.LogWarning($"⚠️ Track '{trackName}' wurde in der MusicLibrary nicht gefunden.");
        return null;
    }
}