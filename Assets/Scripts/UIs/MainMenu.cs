using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Start()
    {   
        ///////// Insert Musik /////////
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic("Main Menu");
        }
        else
        {
            Debug.LogWarning("⚠️ MusicManager wurde in der MainMenu-Szene nicht gefunden!");
        }
    }

    public void Play()
    {
        ///////// Insert Musik /////////
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic("Dungeon");
        }

        SceneManager.LoadScene("SampleScene");
    }

    public void Quit()
    {
        Application.Quit();
    }
}