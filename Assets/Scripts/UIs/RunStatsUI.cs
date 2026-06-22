using TMPro;
using UnityEngine;

public class RunStatsUI : MonoBehaviour
{
    [Header("Texts")]
    public TMP_Text killsText;
    public TMP_Text timeText;
    public TMP_Text depthText;

    public void UpdateStats()
    {
        if (RunStatsManager.Instance == null)
            return;

        killsText.text = "Enemies Killed: " +
                         RunStatsManager.Instance.enemiesKilled;

        timeText.text = "Time Survived: " +
                        RunStatsManager.Instance.GetFormattedTime();

        depthText.text = "Dungeon Depth: " +
                         RunStatsManager.Instance.dungeonDepth;
    }
}