using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void Start()
    {   
        /////////Insert Musik///////////////////////
        MusicManager.Instance.PlayMusic("Main Menu");
    }
     public void Play()
    {
        
        SceneManager.LoadScene("SampleScene");
        
        /////////Insert Musik///////////////////////
        MusicManager.Instance.PlayMusic("Dungeon Theme");

    }

    
     public void Quit()
    {
        
        Application.Quit();

    }

    
}
