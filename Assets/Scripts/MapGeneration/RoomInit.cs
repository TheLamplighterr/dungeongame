using Unity.VisualScripting;
using UnityEngine;

public class RoomInit : MonoBehaviour
{

    public MapManager mapManager;
    public TreeBranch branch;

    public BossRoomInit bossRoomInit;

    public int simpleX;
    public int simpleY;

    public GameObject Me;
    
    public GameObject opening1;
    public GameObject opening2;
    public GameObject opening3;
    public GameObject opening4;

    public Transform mainTransform;// = Me.transform;

    private void importantSetup()
    {
        mapManager = FindObjectOfType<MapManager>();
        branch = mapManager.GetQuadTree();

        mainTransform = Me.transform;
    }

    public int path;
    public int[] openings;

    private void calculatePos()
    {
        float scale = MapManager.GetMapScale();
        float distance = MapManager.GetMapDistance();

        float tempX = mainTransform.position.x / (scale * distance);
        float tempY = mainTransform.position.z / (scale * distance);

        mainTransform.localScale = new Vector3(scale, scale, scale);

        simpleX = (int)tempX;
        simpleY = (int)tempY;
    }

    private void linkToTree()
    {
        branch.linkRoom(Me, this, path);
    }

    private void applyopenings()
    {
        if (openings != null && openings.Length == 4)
        {
            if (openings[0] == 1)
            {
                if(opening1 != null)
                    opening1.SetActive(false);
            }
            if (openings[1] == 1)
            {
                if (opening2 != null)
                    opening2.SetActive(false);
            }
            if (openings[2] == 1)
            {
                if (opening3 != null)
                    opening3.SetActive(false);
            }
            if (openings[3] == 1)
            {
                if (opening4 != null)
                    opening4.SetActive(false);
            }
        }
        else { Debug.Log("openings array null or wrong :/"); }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        importantSetup();
        calculatePos();
        path = mapManager.deliverSimplePathFromXY(simpleX, simpleY);
        openings = mapManager.deliverRoomOpeningsForXY(simpleX, simpleY);

        

        linkToTree();
        applyopenings();


    }

    public void destroySelf()
    {
        if(bossRoomInit != null)
        {
            bossRoomInit.destroySelf();
        }
        GameObject.Destroy(Me);
    }
    
}
