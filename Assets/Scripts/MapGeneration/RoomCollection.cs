using UnityEngine;

public class RoomCollection : MonoBehaviour{

    public GameObject[] connectorRooms = new GameObject[0];

    public GameObject[] spawnRooms = new GameObject[0];

    public GameObject[] poiRooms = new GameObject[0];

    public GameObject[] treasureRooms = new GameObject[0];

    public GameObject[] bossRooms = new GameObject[0];

    public GameObject[] stairsRoom = new GameObject[0];


    public GameObject getRandomRoom(int roomType)
    {

        switch(roomType){
            case 1: return connectorRooms[MapManager.randomI(0,connectorRooms.Length-1)];
            case 2: return spawnRooms[MapManager.randomI(0, spawnRooms.Length - 1)];
            case 3: return poiRooms[MapManager.randomI(0, poiRooms.Length - 1)];
            case 4: return treasureRooms[MapManager.randomI(0, treasureRooms.Length - 1)];
            case 5: return bossRooms[MapManager.randomI(0, bossRooms.Length - 1)];
            case 6: return stairsRoom[MapManager.randomI(0, stairsRoom.Length - 1)];
                default: return connectorRooms[MapManager.randomI(0, connectorRooms.Length - 1)];
        }
    }
}
