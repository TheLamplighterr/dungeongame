using UnityEngine;

public class SceneMusicTrigger : MonoBehaviour
{
    [SerializeField] private string trackToPlay = "Dungeon Theme";

    private void Start()
    {
        // Greift sicher auf Instance zu (erstellt den Manager selbst, falls er fehlt)
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic(trackToPlay);
        }
    }
}