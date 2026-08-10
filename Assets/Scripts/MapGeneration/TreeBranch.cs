using Unity.VisualScripting;
using UnityEngine;

public class TreeBranch : MonoBehaviour
{
    public MapManager mapManager;
    public RoomCollection roomCollection;

    private void Awake()
    {
        if (mapManager == null)
        {
            mapManager = FindObjectOfType<MapManager>();
            roomCollection = mapManager.GetRoomCollection();
        }
    }

    public TreeBranch GetTreeBranch()
    {
        return this;
    }

    private static int layersForLeaf = 3;       //how many layers before it is a leaf

    private int layer;
    private bool isLeaf;

    public bool linked = false;

    public TreeBranch upperLeft = null;
    public TreeBranch upperRight = null;
    public TreeBranch lowerLeft = null;
    public TreeBranch lowerRight = null;

    public GameObject room = null;              //beim spielfeld zugewiesener Raum
    public RoomInit roomInit;

    public TreeBranch(int layer, MapManager mapmanager, RoomCollection roomcollection)        //make a new tree 
    {
        if (layer == layersForLeaf)
        {
            this.isLeaf = true;
        }
        else
        {
            this.isLeaf = false;
        }
        this.layer = layer;
        this.mapManager = mapmanager;
        this.roomCollection = roomcollection;
    }

    public void createNewBranch(int dir)            //branch helper (erstellt branch in der richtung mit automatischer layer und Verweisen)
    {
        if (dir == 1)
        {
            upperLeft = new TreeBranch(this.layer + 1, this.mapManager, this.roomCollection);
            /*
            GameObject branchPart = new GameObject();
            branchPart.AddComponent<TreeBranch>();
            upperLeft = branchPart.GetComponent<TreeBranch>();
            upperLeft = new TreeBranch(this.layer + 1, this.mapManager, this.roomCollection);
            */
        }
        else if (dir == 2)
        {
            upperRight = new TreeBranch(this.layer + 1, this.mapManager, this.roomCollection);
            /*
            GameObject branchPart = new GameObject();
            branchPart.AddComponent<TreeBranch>();
            upperRight = branchPart.GetComponent<TreeBranch>();
            upperRight = new TreeBranch(this.layer + 1, this.mapManager, this.roomCollection);
            */
        }
        else if (dir == 3)
        {
            lowerLeft = new TreeBranch(this.layer + 1, this.mapManager, this.roomCollection);
            /*
            GameObject branchPart = new GameObject();
            branchPart.AddComponent<TreeBranch>();
            lowerLeft = branchPart.GetComponent<TreeBranch>();
            lowerLeft = new TreeBranch(this.layer + 1, this.mapManager, this.roomCollection);
            */
        }
        else if (dir == 4)
        {
            lowerRight = new TreeBranch(this.layer + 1, this.mapManager, this.roomCollection);
            /*
            GameObject branchPart = new GameObject();
            branchPart.AddComponent<TreeBranch>();
            lowerRight = branchPart.GetComponent<TreeBranch>();
            lowerRight = new TreeBranch(this.layer + 1, this.mapManager, this.roomCollection);
            */
        }
    }

    public void createRoom(int type, int path, int roomID)                      //laufe den Pfad und erstelle am Ende den Raum
    {
        if (path == 0)                  //Pfad ist gelaufen
        {
            room = roomCollection.getRandomRoom(type);              //hole dir ein Raum preset
            int[] temp = mapManager.getRoomCoordsFromID(roomID);    //hole dir SimpleMap X und Y
            Instantiate(room, convertXYMapToV3(temp[0], temp[1]), Quaternion.identity);     //übersetze SimpleMap X und Y zu Vector3 x und z
        }
        else if (path != 0)             //Pfad nicht abgelaufen
        {
            if(path % 10 == 1)          //wohin geht der Pfad?
            {
                if(upperLeft == null)   //ist da schon ein branch?
                {
                    createNewBranch(1); //falls nein erstelle neuen branch
                }
                upperLeft.createRoom(type, path / 10, roomID);  //platziere den Raum
            }
            else if (path % 10 == 2)    
            {
                if (upperRight == null)
                {
                    createNewBranch(2);
                }
                upperRight.createRoom(type, path / 10, roomID);
            }
            else if (path % 10 == 3)
            {
                if (lowerLeft == null)
                {
                    createNewBranch(3);
                }
                lowerLeft.createRoom(type, path / 10, roomID);
            }
            else if (path % 10 == 4)
            {
                if (lowerRight == null)
                {
                    createNewBranch(4);
                }
                lowerRight.createRoom(type, path / 10, roomID);
            }


        }
    }






    public Vector3 convertXYMapToV3(int xRaw, int yRaw) 
    { 
        float x  = (float)xRaw * MapManager.GetMapScale() * MapManager.GetMapDistance();
        float y = (float)yRaw * MapManager.GetMapScale() * MapManager.GetMapDistance();
        return new Vector3(x, 0, y);
    }




    public void linkRoom(GameObject givenRoomObject, RoomInit givenRoomInit, int path)                      //laufe den Pfad und erstelle am Ende den Raum
    {
        //Debug.Log("linkRequest recieved: " + path);
        if (path == 0)                  //Pfad ist gelaufen
        {
            room = givenRoomObject;
            roomInit = givenRoomInit;
            Debug.Log("Linked " + givenRoomObject.name + " to quadtree");
            linked = true;
        }
        else if (path != 0)             //Pfad nicht abgelaufen
        {

            if (path % 10 == 1)          //wohin geht der Pfad?
            {
                if (upperLeft != null)   //ist da schon ein branch?
                {
                    Debug.Log("UL");
                    upperLeft.linkRoom(givenRoomObject, givenRoomInit, path / 10);
                }
            }
            else if (path % 10 == 2)
            {
                if (upperRight != null)   //ist da schon ein branch?
                {
                    Debug.Log("UR");
                    upperRight.linkRoom(givenRoomObject, givenRoomInit, path / 10);
                }
            }
            else if (path % 10 == 3)
            {
                if (lowerLeft != null)   //ist da schon ein branch?
                {
                    Debug.Log("BL");
                    lowerLeft.linkRoom(givenRoomObject, givenRoomInit, path / 10);
                }
            }
            else if (path % 10 == 4)
            {
                if (lowerRight != null)   //ist da schon ein branch?
                {
                    Debug.Log("BR");
                    lowerRight.linkRoom(givenRoomObject, givenRoomInit, path / 10);
                }
            }


        }
    }


    public int destroyAllLinkedRooms()
    {
        if (linked)
        {
            //Object.Destroy(room);
            roomInit.destroySelf();
            Debug.Log("ExecutedSelfDestroy");
            roomInit = null;
            room = null;
            linked = false;
        }
        

        int expectedReturns = 0;
        int recievedReturns = 0;

        int rValue = 0;

        if (lowerLeft == null)
        {
            createNewBranch(1);
        }
        if (upperLeft != null)
        {
            expectedReturns++;
            recievedReturns = recievedReturns + upperLeft.destroyAllLinkedRooms();
        }
        if (lowerLeft == null)
        {
            createNewBranch(2);
        }
        if (upperRight != null)
        {
            expectedReturns++;
            recievedReturns = recievedReturns + upperRight.destroyAllLinkedRooms();
        }
        if (lowerLeft == null)
        {
            createNewBranch(3);
        }
        if (lowerLeft != null)
        {
            expectedReturns++;
            recievedReturns = recievedReturns + lowerLeft.destroyAllLinkedRooms();
        }
        if (lowerLeft == null)
        {
            createNewBranch(4);
        }
        if (lowerRight != null) 
        {
            expectedReturns++;
            recievedReturns = recievedReturns + lowerRight.destroyAllLinkedRooms();
        }

        if(recievedReturns == expectedReturns)
        {
            upperLeft = null;
            upperRight = null;
            lowerLeft = null;
            lowerRight = null;
            rValue = 1;
        }


        return rValue;
    }




}
