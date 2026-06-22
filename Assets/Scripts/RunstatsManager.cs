using UnityEngine;

public class RunStatsManager : MonoBehaviour
{
    public static RunStatsManager Instance;

    [Header("Run Stats")]
    public int enemiesKilled;
    public int dungeonDepth;
    public float runTime;

    private bool runActive = true;

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
         Debug.Log("Enemy killed! Total Kills: " + enemiesKilled);
    }

    public void SetDepth(int depth)
    {
        dungeonDepth = depth;
    }

    public void EndRun()
    {
        runActive = false;

        Debug.Log("=== RUN ENDET ===");
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
}