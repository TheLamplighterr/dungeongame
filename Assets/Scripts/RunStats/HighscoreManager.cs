using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class HighscoreManager : MonoBehaviour
{
    public static HighscoreManager Instance { get; private set; }

    private string filePath;
    private HighscoreListWrapper highscoreWrapper = new HighscoreListWrapper();

    // Gibt IMMER den in der JSON gespeicherten letzten Run zurück
    public RunData LatestRun 
    { 
        get 
        { 
            LoadHighscores();
            return highscoreWrapper != null ? highscoreWrapper.lastRun : null; 
        } 
    }

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
        LoadHighscores(); // Aktuellen Stand laden

        RunData newRun = new RunData(score, enemies, playTime, floor);

        // Speichere ihn als dauerhaft letzten Run
        highscoreWrapper.lastRun = newRun;

        // Zur allgemeinen Highscore-Liste hinzufügen
        if (highscoreWrapper.runs == null) highscoreWrapper.runs = new List<RunData>();
        highscoreWrapper.runs.Add(newRun);
        
        // Nach Score sortieren (Höchster zuerst)
        highscoreWrapper.runs = highscoreWrapper.runs.OrderByDescending(r => r.totalScore).ToList();

        SaveHighscores();
        return newRun;
    }

    public List<RunData> GetTopRuns(int count = 5)
    {
        LoadHighscores(); 
        if (highscoreWrapper?.runs == null) return new List<RunData>();
        return highscoreWrapper.runs.Take(count).ToList();
    }

    // Sucht nach der Rank-Position basierend auf der runID
    public int GetRunRank(RunData run)
    {
        if (run == null || string.IsNullOrEmpty(run.runID)) return -1;

        LoadHighscores();
        if (highscoreWrapper?.runs == null) return -1;

        int index = highscoreWrapper.runs.FindIndex(r => r.runID == run.runID);
        return index != -1 ? index + 1 : -1;
    }

    private void SaveHighscores()
    {
        string json = JsonUtility.ToJson(highscoreWrapper, true);
        File.WriteAllText(filePath, json);
        Debug.Log("💾 Highscores & LastRun gespeichert unter: " + filePath);
    }

    private void LoadHighscores()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            highscoreWrapper = JsonUtility.FromJson<HighscoreListWrapper>(json);

            if (highscoreWrapper == null) highscoreWrapper = new HighscoreListWrapper();
            if (highscoreWrapper.runs == null) highscoreWrapper.runs = new List<RunData>();
        }
        else
        {
            highscoreWrapper = new HighscoreListWrapper();
        }
    }
}