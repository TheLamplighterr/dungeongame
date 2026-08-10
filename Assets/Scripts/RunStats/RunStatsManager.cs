using UnityEngine;

public class RunStatsManager : MonoBehaviour
{
    public static RunStatsManager Instance;

    [Header("Run Stats")]
    public int enemiesKilled;
    public int dungeonDepth = 1;
    public float runTime;

    private bool runActive = true;

    // --- AUTOMATISCHER SCORE ---
    // Berechnet den Score dynamisch, sobald irgendwer 'currentScore' abfragt!
    public int currentScore => CalculateScore();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (runActive)
        {
            runTime += Time.deltaTime;
        }
    }

    public void AddKill()
    {
        enemiesKilled++;
    }

    public void SetDepth(int depth)
    {
        dungeonDepth = depth;
    }

    public void UpdateCurrentLevel(int newLevel)
    {
        dungeonDepth = newLevel;
    }

    public int CalculateScore()
    {
        int killScore = enemiesKilled * 150;
        int depthScore = dungeonDepth * 1000;
        
        // Zeitbonus (2000 Startpunkte, -2 Punkte pro Sekunde)
        int baseTimeBonus = 2000;
        int timePenalty = Mathf.FloorToInt(runTime * 2f);
        int timeScore = Mathf.Max(0, baseTimeBonus - timePenalty);

        return killScore + depthScore + timeScore;
    }

    public void EndRun()
    {
        runActive = false;

        Debug.Log($"=== RUN ENDET === Score: {currentScore} | Kills: {enemiesKilled} | Zeit: {GetFormattedTime()} | Tiefe: {dungeonDepth}");

        if (HighscoreManager.Instance != null)
        {
            HighscoreManager.Instance.AddRun(currentScore, enemiesKilled, runTime, dungeonDepth);
        }
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(runTime / 60);
        int seconds = Mathf.FloorToInt(runTime % 60);
        return $"{minutes:00}:{seconds:00}";
    }

    public void ResetRun()
    {
        enemiesKilled = 0;
        dungeonDepth = 1;
        runTime = 0;
        runActive = true;
    }
}