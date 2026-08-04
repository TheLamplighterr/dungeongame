using UnityEngine;

public class AudioDiagnostic : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== 🔍 AUDIO DIAGNOSE START ===");
        
        // 1. Check MusicManager Script
        MusicManager manager = GetComponent<MusicManager>();
        if (manager == null) Debug.LogError("❌ Fehler 1: Das Skript 'MusicManager.cs' fehlt auf diesem GameObject!");
        else Debug.Log("✅ Check 1: MusicManager-Skript ist auf dem Objekt.");

        // 2. Check AudioSource
        AudioSource source = GetComponent<AudioSource>();
        if (source == null) Debug.LogError("❌ Fehler 2: Keine 'AudioSource'-Komponente auf diesem GameObject gefunden!");
        else
        {
            Debug.Log("✅ Check 2: AudioSource vorhanden.");
            if (source.spatialBlend > 0) Debug.LogWarning($"⚠️ Warnung: Spatial Blend steht auf {source.spatialBlend} (sollte 0.0 / 2D sein!).");
        }

        // 3. Check MusicLibrary
        MusicLibrary lib = GetComponent<MusicLibrary>();
        if (lib == null) Debug.LogError("❌ Fehler 3: 'MusicLibrary'-Skript fehlt auf diesem GameObject!");
        else
        {
            Debug.Log("✅ Check 3: MusicLibrary vorhanden.");
            if (lib.tracks == null || lib.tracks.Length == 0) Debug.LogError("❌ Fehler 3b: Die MusicLibrary enthält 0 Tracks!");
        }

        // 4. Check Singleton Instance
        if (MusicManager.Instance == null) Debug.LogError("❌ Fehler 4: MusicManager.Instance ist NULL!");
        else Debug.Log("✅ Check 4: MusicManager.Instance ist erfolgreich gesetzt!");

        Debug.Log("=== 🔍 AUDIO DIAGNOSE ENDE ===");
    }
}