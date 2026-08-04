using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HighscoreUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform tableParent;
    public HighscoreEntryUI entryPrefab;
    public GameObject extraRunContainer;
    public HighscoreEntryUI extraRunEntry;

    [Header("Detail Panel References")]
    public GameObject detailPanel;
    public TMP_Text detailScoreText;
    public TMP_Text detailDateText;
    public TMP_Text detailEnemiesText;
    public TMP_Text detailTimeText;
    public TMP_Text detailFloorText;

    [Header("Detail Panel Animation Settings")]
    public float slideDuration = 0.35f; // Dauer in Sekunden
    public Vector2 slideFromOffset = new Vector2(1000f, 0f); // X: 1000 = Schiebt von rechts rein

    private RectTransform detailRectTransform;
    private Vector2 detailTargetPosition;
    private Coroutine currentSlideCoroutine;

    private void Awake()
    {
        if (detailPanel != null)
        {
            detailRectTransform = detailPanel.GetComponent<RectTransform>();
            if (detailRectTransform != null)
            {
                // Ziel-Position (in der Mitte) einmalig sichern
                detailTargetPosition = detailRectTransform.anchoredPosition;
            }
        }
    }

    private void OnEnable()
    {
        if (HighscoreManager.Instance != null)
        {
            RefreshBoard();
        }
        
        // Beim Einschalten das Detail-Panel sofort ohne Animation verstecken
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
    }

    public void RefreshBoard()
    {
        if (HighscoreManager.Instance == null || tableParent == null) return;

        // 1. Alte Einträge löschen
        foreach (Transform child in tableParent)
        {
            Destroy(child.gameObject);
        }

        var topRuns = HighscoreManager.Instance.GetTopRuns(5);
        RunData latest = HighscoreManager.Instance.LatestRun;
        bool latestInTop5 = false;

        // 2. Top 5 erzeugen
        for (int i = 0; i < topRuns.Count; i++)
        {
            RunData run = topRuns[i];
            if (entryPrefab == null) continue;

            HighscoreEntryUI entry = Instantiate(entryPrefab, tableParent);
            bool isLatest = (latest != null && run.runID == latest.runID);

            if (isLatest) latestInTop5 = true;

            entry.Setup(i + 1, run, isLatest, this);
        }

        // 3. Extra Run Zeile falls nicht unter den Top 5
        if (extraRunContainer != null && extraRunEntry != null)
        {
            if (latest != null && !latestInTop5)
            {
                extraRunContainer.SetActive(true);
                int rank = HighscoreManager.Instance.GetRunRank(latest);
                extraRunEntry.Setup(rank, latest, true, this);
            }
            else
            {
                extraRunContainer.SetActive(false);
            }
        }
    }

    public void ShowDetails(RunData data)
    {
        if (detailPanel == null) return;

        // Texte mit den Daten des geklickten Runs füllen
        if (detailScoreText) detailScoreText.text = "Score: " + data.totalScore.ToString("N0");
        if (detailDateText) detailDateText.text = "Datum: " + data.dateTimeString;
        if (detailEnemiesText) detailEnemiesText.text = "Besiegte Gegner: " + data.enemiesKilled;

        if (detailTimeText != null)
        {
            int minutes = Mathf.FloorToInt(data.playTimeInSeconds / 60F);
            int seconds = Mathf.FloorToInt(data.playTimeInSeconds % 60F);
            detailTimeText.text = string.Format("Spieldauer: {0:00}:{1:00}", minutes, seconds);
        }

        if (detailFloorText) detailFloorText.text = "Tiefe: " + data.highestFloor;

        // Visuals vorbereiten
        detailPanel.SetActive(true);
        detailPanel.transform.SetAsLastSibling(); // Ganz nach vorne legen

        // Animation starten (Rein-Sliden)
        if (currentSlideCoroutine != null) StopCoroutine(currentSlideCoroutine);
        currentSlideCoroutine = StartCoroutine(SlidePanel(true));
    }

    public void CloseDetails()
    {
        if (detailPanel == null || !detailPanel.activeSelf) return;

        // Animation starten (Raus-Sliden)
        if (currentSlideCoroutine != null) StopCoroutine(currentSlideCoroutine);
        currentSlideCoroutine = StartCoroutine(SlidePanel(false));
    }

    private IEnumerator SlidePanel(bool slideIn)
    {
        if (detailRectTransform == null) yield break;

        Vector2 startPos = slideIn ? (detailTargetPosition + slideFromOffset) : detailTargetPosition;
        Vector2 endPos = slideIn ? detailTargetPosition : (detailTargetPosition + slideFromOffset);

        float time = 0f;

        while (time < slideDuration)
        {
            time += Time.unscaledDeltaTime; // Funktioniert auch bei Time.timeScale = 0
            float t = time / slideDuration;

            // Sanftes Abbremsen am Ende (Ease-Out)
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            detailRectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        detailRectTransform.anchoredPosition = endPos;

        // Nach dem Herausschieben das Panel inaktiv schalten
        if (!slideIn)
        {
            detailPanel.SetActive(false);
        }
    }
}