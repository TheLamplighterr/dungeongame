using System;
using System.Collections.Generic;

[Serializable]
public class RunData
{
    public int totalScore;
    public string dateTimeString;
    
    public int enemiesKilled;
    public float playTimeInSeconds;
    public int highestFloor;
    
    public string runID;

    public RunData(int score, int enemies, float playTime, int floor)
    {
        totalScore = score;
        enemiesKilled = enemies;
        playTimeInSeconds = playTime;
        highestFloor = floor;
        dateTimeString = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        runID = Guid.NewGuid().ToString();
    }
}

// Hilfsklasse für den JSON-Serializer von Unity
[Serializable]
public class HighscoreListWrapper
{
    public List<RunData> runs = new List<RunData>();
    public RunData lastRun; // <-- JETZT NEU: Der allerletzte Run wird hier dauerhaft in der JSON gespeichert!
}