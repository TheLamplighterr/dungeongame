using UnityEngine;

public class RunStatsManager : MonoBehaviour
{
    public static RunStatsManager Instance;

    [Header("Run Stats")]
    public int enemiesKilled;
    public int dungeonDepth;
    public float runTime;

    private bool runActive = true;

    [Header("Score")]
    public int currentScore;

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
         // DEBUG TEST
         if (Input.GetKeyDown(KeyCode.P))
        {
        enemiesKilled += 3;
        dungeonDepth = 2;

        currentScore = CalculateScore();

        Debug.Log("TEST SCORE: " + currentScore);
        }
    }

    public void AddKill()
    {
        enemiesKilled++;
         Debug.Log("Enemy killed! Total Kills: " + enemiesKilled);
    }

    public void SetDepth(int depth)
    {
        dungeonDepth = depth;
    }

    public void EndRun()
    {
        runActive = false;

        currentScore = CalculateScore();

        Debug.Log("=== RUN ENDET ===");
        Debug.Log("Score: " + currentScore);
        Debug.Log("Kills: " + enemiesKilled);
        Debug.Log("Zeit: " + GetFormattedTime());
        Debug.Log("Tiefe: " + dungeonDepth);
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

    public int CalculateScore()
{
    int score = 0;

    // Kills sind wichtig
    score += enemiesKilled * 100;

    // Tiefe ist sehr wichtig
    score += dungeonDepth * 500;

    // Zeitbonus (je schneller desto besser)
    score += Mathf.Max(0, 1000 - Mathf.FloorToInt(runTime));

    return score;
}
}