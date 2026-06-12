using UnityEngine;

public class TreeBranch : MonoBehaviour
{
    public MapManager mapManager;
    public RoomCollection roomCollection;

    private static int layersForLeaf = 3;       //how many layers before it is a tree



    private int layer;
    private bool isLeaf;

    public TreeBranch upperLeft = null;
    public TreeBranch upperRight = null;
    public TreeBranch lowerLeft = null;
    public TreeBranch lowerRight = null;

    public GameObject room = null;              //beim spielfeld zugewiesener raum

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

    public void createNewBranch(int dir)            //branch helper (erstellt branch in der richtung mit automatischer layer und verweisen)
    {
        if (dir == 1)
        {
            upperLeft = new TreeBranch(this.layer + 1, this.mapManager, this.roomCollection);
        }
        else if (dir == 2)
        {
            upperRight = new TreeBranch(this.layer + 1, this.mapManager, this.roomCollection);
        }
        else if (dir == 3)
        {
            lowerLeft = new TreeBranch(this.layer + 1, this.mapManager, this.roomCollection);
        }
        else if (dir == 4)
        {
            lowerRight = new TreeBranch(this.layer + 1, this.mapManager, this.roomCollection);
        }
    }

    public void createRoom(int type, int path, int roomID)                      //laufe den pfad und erstelle am ende den raum
    {
        if (path == 0)                  //pfad ist gelaufen
        {
            room = roomCollection.getRandomRoom(type);              //hole dir ein raum preset
            int[] temp = mapManager.getRoomCoordsFromID(roomID);    //hole dir simpleMap X und y
            Instantiate(room, convertXYMapToV3(temp[0], temp[1]), Quaternion.identity);     //übersetze SimpleMap X und Y zu Vector3 x und z
        }
        else if (path != 0)             //pfad nicht abgelaufen
        {
            if(path % 10 == 1)          //wohin geht der pfad?
            {
                if(upperLeft == null)   //ist da schon ein branch?
                {
                    createNewBranch(1); //falls nein erstelle neuen branch
                }
                upperLeft.createRoom(type, path / 10, roomID);  //platziere den raum
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


}
