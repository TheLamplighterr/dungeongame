using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HighscoreEntryUI : MonoBehaviour
{
    [Header("Text Felder")]
    public TMP_Text rankText;
    public TMP_Text scoreText;
    public TMP_Text dateText;

    [Header("Farben")]
    public Image backgroundImage;
    public Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    public Color latestRunColor = new Color(0.8f, 0.6f, 0.1f, 0.8f);

    private RunData boundData;
    private HighscoreUI mainUI;

    public void Setup(int rank, RunData data, bool isLatestRun, HighscoreUI ui)
    {
        boundData = data;
        mainUI = ui;

        if (rankText) rankText.text = "#" + rank;
        if (scoreText) scoreText.text = data.totalScore.ToString("N0");
        if (dateText) dateText.text = data.dateTimeString;

        if (backgroundImage != null)
        {
            backgroundImage.color = isLatestRun ? latestRunColor : normalColor;
        }

        // Klick-Event automatisch beim Erstellen an den Button hängen:
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnRowClicked);
        }
    }

    public void OnRowClicked()
{
    Debug.Log("🎯 ZEILE GEKLICKT: " + (boundData != null ? boundData.totalScore.ToString() : "Keine Daten"));

    if (boundData != null && mainUI != null)
    {
        mainUI.ShowDetails(boundData);
    }
    else
    {
        Debug.LogWarning("⚠️ mainUI oder boundData ist NULL!");
    }
}
}