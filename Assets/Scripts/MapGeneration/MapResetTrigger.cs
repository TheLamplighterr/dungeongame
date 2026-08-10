using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MapResetTrigger : MonoBehaviour
{
    private MapManager mapManager;

    [Header("UI Namen in der Szene")]
    [Tooltip("Der exakte Name deines Prompt UI-Objekts in der Scene-Hierarchy")]
    public string uiObjectName = "EndRoomPromptUI";

    [Tooltip("Der exakte Name des schwarze Fade-Image-Objekts im Canvas")]
    public string fadeImageName = "ScreenFadeImage";

    [Header("Einstellungen für Beenden (Taste F)")]
    [Tooltip("Name deiner Hauptmenü-Szene im Build Settings Fenster")]
    public string mainMenuSceneName = "MainMenu";

    [Header("UI Prompt Fade Einstellungen")]
    [Tooltip("Dauer des Ein- und Ausfadens der Text-Prompt in Sekunden")]
    public float promptFadeDuration = 0.3f;

    [Header("Teleport VFX & Timing")]
    [Tooltip("Partikelsystem Prefab für den leuchtenden Teleport-Kreis")]
    public GameObject teleportVFXPrefab;

    [Tooltip("Verzögerung in Sekunden, bis die Ebene geladen / das Menü aufgerufen wird")]
    public float transitionDelay = 1.5f;

    [Header("VFX Spawn Marker (Optional)")]
    [Tooltip("Exakter Name des leeren GameObjects im Raum-Prefab, wo das VFX entstehen soll")]
    public string vfxSpawnPointName = "TeleportVFXSpawnPoint";
    public Vector3 vfxOffset = new Vector3(0, 0.05f, 0);

    [Header("Audio SFX")]
    [Tooltip("Magischer Soundeffekt (Swoosh/Teleport) beim Interagieren")]
    public AudioClip interactSFX;

    [Range(0f, 1f)]
    [Tooltip("Lautstärke des Soundeffekts (0 = stumm, 1 = volle Lautstärke)")]
    public float soundVolume = 1.0f;

    private GameObject promptUI;
    private CanvasGroup canvasGroup;
    private Image screenFadeImage;
    private Coroutine fadeCoroutine;
    private bool isPlayerInRange = false;
    private bool isTransitioning = false;
    private Transform customSpawnPoint;
    private Transform playerTransform;

    void Start()
    {
        mapManager = FindFirstObjectByType<MapManager>();
        FindPromptUI();
        FindScreenFadeImage();
        FindSpawnPoint();
    }

    private void FindPromptUI()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (Canvas canvas in canvases)
        {
            Transform foundTransform = canvas.transform.Find(uiObjectName);
            if (foundTransform != null)
            {
                promptUI = foundTransform.gameObject;
                break;
            }
        }

        if (promptUI != null)
        {
            canvasGroup = promptUI.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = promptUI.AddComponent<CanvasGroup>();
            }

            promptUI.SetActive(true);
            canvasGroup.alpha = 0f;
        }
    }

    private void FindScreenFadeImage()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (Canvas canvas in canvases)
        {
            Transform foundTransform = canvas.transform.Find(fadeImageName);
            if (foundTransform != null)
            {
                screenFadeImage = foundTransform.GetComponent<Image>();
                break;
            }
        }

        if (screenFadeImage != null)
        {
            Debug.Log("<color=green>[ScreenFade]</color> Fade Image erfolgreich gefunden!");
            Color c = screenFadeImage.color;
            c.a = 0f;
            screenFadeImage.color = c;
            screenFadeImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError($"<color=red>[ScreenFade]</color> Konnte kein Image mit dem Namen '{fadeImageName}' im Canvas finden!");
        }
    }

    private void FindSpawnPoint()
    {
        Transform root = transform.root;
        Transform[] allTransforms = root.GetComponentsInChildren<Transform>(true);

        foreach (Transform t in allTransforms)
        {
            if (t.name == vfxSpawnPointName)
            {
                customSpawnPoint = t;
                break;
            }
        }
    }

    void Update()
    {
        if (isPlayerInRange && !isTransitioning)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(NextLevelSequence());
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                StartCoroutine(ExitGameSequence());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTransitioning)
        {
            isPlayerInRange = true;
            playerTransform = other.transform;

            if (promptUI == null) FindPromptUI();

            if (canvasGroup != null)
            {
                StartPromptFade(1f);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !isTransitioning)
        {
            isPlayerInRange = false;

            if (canvasGroup != null)
            {
                StartPromptFade(0f);
            }
        }
    }

    private void StartPromptFade(float targetAlpha)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadePromptRoutine(targetAlpha));
    }

    private IEnumerator FadePromptRoutine(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < promptFadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / promptFadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    private IEnumerator FadeScreenToBlack(float duration)
    {
        if (screenFadeImage == null) yield break;

        float time = 0f;
        Color color = screenFadeImage.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, time / duration);
            screenFadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        screenFadeImage.color = color;
    }

    private void PlayInteractSound()
    {
        if (interactSFX != null)
        {
            Vector3 soundPos = (customSpawnPoint != null) ? customSpawnPoint.position : transform.position;
            AudioSource.PlayClipAtPoint(interactSFX, soundPos, soundVolume);
        }
    }

    private void SpawnTeleportVFX()
    {
        if (teleportVFXPrefab != null)
        {
            Vector3 spawnPos;
            
            if (customSpawnPoint != null)
            {
                spawnPos = customSpawnPoint.position + vfxOffset;
            }
            else if (playerTransform != null)
            {
                spawnPos = playerTransform.position + vfxOffset;
            }
            else
            {
                spawnPos = transform.position + vfxOffset;
            }

            GameObject vfxInstance = Instantiate(teleportVFXPrefab, spawnPos, Quaternion.identity);

            ParticleSystem[] particleSystems = vfxInstance.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                ps.Play();
            }

            Destroy(vfxInstance, 4.0f);
        }
    }

    /// <summary>
    /// Sperrt die Bewegungen und Eingaben des Spielers.
    /// </summary>
    private void DisablePlayerControls()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        if (playerTransform != null)
        {
            // 1. Rigidbody-Geschwindigkeit stoppen (falls vorhanden)
            Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // 2. Skripte auf dem Spieler deaktivieren (z. B. PlayerController, PlayerMovement etc.)
            MonoBehaviour[] scripts = playerTransform.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                // Deaktiviere deine Movement/Control-Skripte (Erweitere die Namen bei Bedarf)
                string scriptName = script.GetType().Name.ToLower();
                if (scriptName.Contains("player") || scriptName.Contains("controller") || scriptName.Contains("movement") || scriptName.Contains("input"))
                {
                    script.enabled = false;
                }
            }
        }
    }

    /// <summary>
    /// Reaktiviert die Bewegungen des Spielers für das neue Level.
    /// </summary>
    private void EnablePlayerControls()
    {
        if (playerTransform != null)
        {
            MonoBehaviour[] scripts = playerTransform.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                string scriptName = script.GetType().Name.ToLower();
                if (scriptName.Contains("player") || scriptName.Contains("controller") || scriptName.Contains("movement") || scriptName.Contains("input"))
                {
                    script.enabled = true;
                }
            }
        }
    }

    private IEnumerator NextLevelSequence()
    {
        isTransitioning = true;

        // Spieler-Steuerung sofort sperren!
        DisablePlayerControls();

        if (canvasGroup != null)
            StartPromptFade(0f);

        PlayInteractSound();
        SpawnTeleportVFX();

        // Startet das sanfte Bildschirmausblenden parallel zur Teleport-Verzögerung
        StartCoroutine(FadeScreenToBlack(transitionDelay));

        yield return new WaitForSeconds(transitionDelay);

        if (mapManager != null)
        {
            // 1. Generiert den neuen Floor (erhöht mapManager.level intern um 1)
            mapManager.generateNewLevel();

            // 2. Aktualisiert den Level-Stand im RunStatsManager
            if (RunStatsManager.Instance != null)
            {
                RunStatsManager.Instance.UpdateCurrentLevel(mapManager.level);
            }
        }
        else
        {
            Debug.LogError("[MapResetTrigger] MapManager wurde in der Szene nicht gefunden!");
        }

        // Steuerung wieder freigeben
        EnablePlayerControls();
        isTransitioning = false;
    }

    private IEnumerator ExitGameSequence()
    {
        isTransitioning = true;

        // Spieler-Steuerung sofort sperren!
        DisablePlayerControls();

        if (canvasGroup != null)
            StartPromptFade(0f);

        PlayInteractSound();
        SpawnTeleportVFX();

        StartCoroutine(FadeScreenToBlack(transitionDelay));

        yield return new WaitForSeconds(transitionDelay);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // --- RUN ENDEN UND HIGHSCORE SPEICHERN ---
        if (RunStatsManager.Instance != null)
        {
            RunStatsManager.Instance.EndRun();
        }
        else
        {
            Debug.LogWarning("[MapResetTrigger] RunStatsManager.Instance ist NULL! Run konnte nicht gespeichert werden.");
        }

        Debug.Log("[MapResetTrigger] Lade Hauptmenü...");

        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Application.Quit();

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
}