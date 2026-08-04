using System;

[Serializable]
public class RunData
{
    public int totalScore;
    public string dateTimeString; // z.B. "03.08.2026 20:15"
    
    // Details für die Ansicht beim Anklicken
    public int enemiesKilled;
    public float playTimeInSeconds;
    public int highestFloor;
    
    // System-ID zur Erkennung des neusten Runs
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
    public System.Collections.Generic.List<RunData> runs = new System.Collections.Generic.List<RunData>();
}