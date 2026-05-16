using UnityEngine;

[System.Serializable]
public struct MusicTrack
{
    public string trackName;
    public AudioClip clip;

}


public class MusicLibrary : MonoBehaviour
{

    public MusicTrack[] tracks;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   public AudioClip GetClipFromName(string trackName)
    {
        foreach(var track in tracks)
        {
            if(track.trackName == trackName)
            {
                return track.clip;
            }

        }
        return null;
    }


}

