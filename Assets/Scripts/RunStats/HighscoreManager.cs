using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class HighscoreManager : MonoBehaviour
{
    public static HighscoreManager Instance { get; private set; }

    private string filePath;
    private HighscoreListWrapper highscoreWrapper = new HighscoreListWrapper();

    public RunData LatestRun { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            filePath = Path.Combine(Application.persistentDataPath, "highscores.json");
            LoadHighscores();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Einen neuen Run am Ende des Spiels hinzufügen & als JSON speichern
    public RunData AddRun(int score, int enemies, float playTime, int floor)
    {
        RunData newRun = new RunData(score, enemies, playTime, floor);
        LatestRun = newRun;

        highscoreWrapper.runs.Add(newRun);
        
        // Nach Score sortieren (Höchster zuerst)
        highscoreWrapper.runs = highscoreWrapper.runs.OrderByDescending(r => r.totalScore).ToList();

        SaveHighscores();
        return newRun;
    }

    public List<RunData> GetTopRuns(int count = 5)
    {
        return highscoreWrapper.runs.Take(count).ToList();
    }

    public int GetRunRank(RunData run)
    {
        return highscoreWrapper.runs.IndexOf(run) + 1; // Rang (1-basiert)
    }

    private void SaveHighscores()
    {
        string json = JsonUtility.ToJson(highscoreWrapper, true);
        File.WriteAllText(filePath, json);
        Debug.Log("💾 Highscores gespeichert unter: " + filePath);
    }

    private void LoadHighscores()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            highscoreWrapper = JsonUtility.FromJson<HighscoreListWrapper>(json);
        }
        else
        {
            highscoreWrapper = new HighscoreListWrapper();
        }
    }
}