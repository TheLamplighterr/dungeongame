using TMPro;
using UnityEngine;

public class RunStatsUI : MonoBehaviour
{
    [Header("Texts")]
    public TMP_Text killsText;
    public TMP_Text timeText;
    public TMP_Text depthText;
    public TMP_Text scoreText;

    public void UpdateStats()
    {
        if (RunStatsManager.Instance == null)
            return;

        killsText.text = "Enemies Killed: " + RunStatsManager.Instance.enemiesKilled;
        timeText.text = "Time Survived: " + RunStatsManager.Instance.GetFormattedTime();
        depthText.text = "Dungeon Depth: " + RunStatsManager.Instance.dungeonDepth;
        scoreText.text = "Score: " + RunStatsManager.Instance.currentScore;

        // =========================================================================
        // NEU: Nutzt genau deine 'runTime' aus dem RunStatsManager
        // =========================================================================
        if (HighscoreManager.Instance != null)
        {
            HighscoreManager.Instance.AddRun(
                score: RunStatsManager.Instance.currentScore,
                enemies: RunStatsManager.Instance.enemiesKilled,
                playTime: RunStatsManager.Instance.runTime, // <--- HIER WAR DER KLAINE UNTERSCHIED!
                floor: RunStatsManager.Instance.dungeonDepth
            );
        }
    }
}