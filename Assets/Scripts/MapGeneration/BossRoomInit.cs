using UnityEngine;
using UnityEngine.UIElements;

public class BossRoomInit : MonoBehaviour
{
    public RoomInit roomInit;
    MapManager mapManager;

    public GameObject Me;
    public GameObject Opening1;
    public GameObject Opening2;
    public GameObject Opening3;
    public GameObject Opening4;

    public Transform mainTransform;

    int dir;

    public float Speed = 2.0f;

    private float scale;

    private void importantSetup()
    {
        mapManager = FindObjectOfType<MapManager>();

        mainTransform = Me.transform;

        scale = MapManager.GetMapScale();

        mainTransform.localScale = new Vector3(scale, scale, scale);

        dir = mapManager.GetBossToStairsDir();
    }

    private void openOpenings()
    {
        if (dir < 5 && dir > 0) 
        {
            for (int i = 1; i < 5; i++)
            {
                if(dir != i)
                {
                    openOne(i);
                }
            }
        }
        else
        {
            for (int i = 1; i < 5; i++)
            {
                openOne(i);
            }
        }
    }
    
    private void openOne(int given)
    {
        if (given == 1)
        {
            if(Opening1 != null)
            {
                Opening1.SetActive(false);
            }
        }
        else if (given == 2) 
        {
            if (Opening2 != null)
            {
                Opening2.SetActive(false);
            }
        }
        else if(given == 3)
        {
            if (Opening3 != null)
            {
                Opening3.SetActive(false);
            }
        }
        else if( given == 4)
        {
            if (Opening4 != null)
            {
                Opening4.SetActive(false);
            }
        }


    }

    public void destroySelf()
    {
        GameObject.Destroy(Me);
    }

    public void bossDefeat()
    {
        for (int i = 1; i < 5; i++)
        {
            openOne(i);
        }
        /*
        float tempSpeed = Speed * scale;
        mainTransform.Translate(Vector3.up * tempSpeed * Time.deltaTime);
        */
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        importantSetup();
        openOpenings();
    }

}
