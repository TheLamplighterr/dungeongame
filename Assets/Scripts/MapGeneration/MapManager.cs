using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public TreeBranch Quadtree;

    private int maxSpielfeldX = 7;
    private int maxSpielfeldY = 7;

    //übersichtsvariablen
    private static int emptyField = 0;     //const
    private static int connectorRoom = 1;  //const
    private static int spawnRoom = 2;      //const
    private static int poiRoom = 3;        //const
    private static int treasureRoom = 4;   //const
    private static int bossRoom = 5;       //const
    private static int stairsRoom = 6;     //const

    //Generelle map einstellungen
    static float scale = 1f;
    static float distance = 10f;

    //einstellungen
    public int[] spawnField = { 0, 0 }; //hier ist der startbereich des Spielers auf deisem level
    public int spawnRoomCount = 1;      //const
    public int poiCount = 2;            //let       Points of interrest (ereignissräume)
    public int treasureRoomCount = 1;   //let       
    public int bossRoomCount = 1;       //const


    //level boundaries
    //adjustGenerationBoundaries

    public static float GetMapScale()
    {
        return scale;
    }
    public static float GetMapDistance() { return distance; }

    List<int[]> roomList = new List<int[]>()        //liste aller aktiven räume mit position und verweis
        {
            new int[4]{spawnRoom,0,0,0}         //type ID X Y
        };
    private static int listRoomType = 0;
    private static int listRoomID = 1;
    private static int listRoomX = 2;
    private static int listRoomY = 3;

    public int[] getRoomCoordsFromID(int givenID) 
    {
        if (roomList.Count > givenID && givenID >= 0)       //wenn der gesuchte raum in der liste ist
        {
            return new int[] { roomList[givenID][2], roomList[givenID][3]};     //gib den x und y wert zurück
        }
        else
        {
            return new int[] { -5, -5 };            //sonst default zu -5/-5 (außerhalb der map)
        }
    }

    public void InitialiseRoomFromList(int listIndex)
    {
        if (roomList.Count > listIndex)
        {
            int treePath = simpleMap[ roomList[listIndex][listRoomX] ][ roomList[listIndex][listRoomY] ][5];
            Quadtree.createRoom(roomList[listIndex][listRoomType], treePath, roomList[listIndex][listRoomID]);
        }
    }
    //Quadtree (actual gameobjects)
    //player proximity
    //load content



    //[x][y][0] =   raumart     (siehe constants oben)
    //[x][y][1] =   verbindung oben /\      0 = zu sonst offen
    //[x][y][2] =   verbindung rechts ->    0 = zu sonst offen
    //[x][y][3] =   verindung unten \/      0 = zu sonst offen
    //[x][y][4] =   verbindung links <-     0 = zu sonst offen
    //[x][y][5] =   Placeholder (zusätzliche Eigenschaften)     //placeholder für leichteren quad tree verweis? (bsp: 143 -> In bereich 1 von bereich 4 von bereich 3)
    int[][][] simpleMap = resetSimpleMap();
    //apply map to quadtree
    //deatroy/create
                            

    /*              (Beispiel)
    a | b | c d     a ist hier 11
    --+-- |         e ist hier 31
    e | f | g h     k ist hier 13
    ------+----     h ist hier 42
    i j   | k l
    m n   | o p
    */

    //translate xyTopos

    private static int[][][] resetSimpleMap()
    {
        return new int[8][][] {
            new int[8][] {
                new int[6] {0,0,0,0,0,111},
                new int[6] {0,0,0,0,0,211},
                new int[6] {0,0,0,0,0,121},
                new int[6] {0,0,0,0,0,221},
                new int[6] {0,0,0,0,0,112},
                new int[6] {0,0,0,0,0,212},
                new int[6] {0,0,0,0,0,122},
                new int[6] {0,0,0,0,0,222}
            },
            new int[8][] {
                new int[6] {0,0,0,0,0,311},
                new int[6] {0,0,0,0,0,411},
                new int[6] {0,0,0,0,0,321},
                new int[6] {0,0,0,0,0,421},
                new int[6] {0,0,0,0,0,312},
                new int[6] {0,0,0,0,0,412},
                new int[6] {0,0,0,0,0,322},
                new int[6] {0,0,0,0,0,422}
            },
            new int[8][] {
                new int[6] {0,0,0,0,0,131},
                new int[6] {0,0,0,0,0,231},
                new int[6] {0,0,0,0,0,141},
                new int[6] {0,0,0,0,0,241},
                new int[6] {0,0,0,0,0,132},
                new int[6] {0,0,0,0,0,232},
                new int[6] {0,0,0,0,0,142},
                new int[6] {0,0,0,0,0,242}
            },
            new int[8][] {
                new int[6] {0,0,0,0,0,331},
                new int[6] {0,0,0,0,0,431},
                new int[6] {0,0,0,0,0,341},
                new int[6] {0,0,0,0,0,441},
                new int[6] {0,0,0,0,0,332},
                new int[6] {0,0,0,0,0,432},
                new int[6] {0,0,0,0,0,342},
                new int[6] {0,0,0,0,0,442}
            },
            new int[8][] {
                new int[6] {0,0,0,0,0,113},
                new int[6] {0,0,0,0,0,213},
                new int[6] {0,0,0,0,0,123},
                new int[6] {0,0,0,0,0,223},
                new int[6] {0,0,0,0,0,114},
                new int[6] {0,0,0,0,0,214},
                new int[6] {0,0,0,0,0,124},
                new int[6] {0,0,0,0,0,224}
            },
            new int[8][] {
                new int[6] {0,0,0,0,0,313},
                new int[6] {0,0,0,0,0,413},
                new int[6] {0,0,0,0,0,323},
                new int[6] {0,0,0,0,0,423},
                new int[6] {0,0,0,0,0,314},
                new int[6] {0,0,0,0,0,414},
                new int[6] {0,0,0,0,0,324},
                new int[6] {0,0,0,0,0,424}
            },
            new int[8][] {
                new int[6] {0,0,0,0,0,133},
                new int[6] {0,0,0,0,0,233},
                new int[6] {0,0,0,0,0,143},
                new int[6] {0,0,0,0,0,243},
                new int[6] {0,0,0,0,0,134},
                new int[6] {0,0,0,0,0,234},
                new int[6] {0,0,0,0,0,144},
                new int[6] {0,0,0,0,0,244}
            },
            new int[8][] {
                new int[6] {0,0,0,0,0,333},
                new int[6] {0,0,0,0,0,433},
                new int[6] {0,0,0,0,0,343},
                new int[6] {0,0,0,0,0,443},
                new int[6] {0,0,0,0,0,334},
                new int[6] {0,0,0,0,0,434},
                new int[6] {0,0,0,0,0,344},
                new int[6] {0,0,0,0,0,444}
            }
        };
    }

    public static int randomI(int x, int y)
    {
        return (int) Random.Range(x, y + 1);
    }

    private void GenerateMap()
    {
        generateSimpleMap();
    }

    private void generateSimpleMap()
    {
        simpleMap = resetSimpleMap();
        roomList.Clear();
        PlaceRoom(spawnRoom, spawnField[0], spawnField[1]);
        PlaceRooms(bossRoom, 1);
        PlaceRooms(poiRoom, poiCount);
        PlaceRooms(treasureRoom, treasureRoomCount);

        //prepareConnections    (assign weights etc)

        //decideConnections     (MST)

        //connect rooms         (try to create a path for each connection)

        //translateToQuadTree   (quadtree leaves mit räumen füllen)
        for (int i = 0; i < roomList.Count; i++) 
        {
            InitialiseRoomFromList(i);
        }

    }

    private void PlaceRooms(int raumart, int raummenge)
    {
        for (int i = 0; i< raummenge; i++) 
        {
            bool success = false;
            while (!success) 
            {
                success = PlaceRoom(raumart, randomI(0,maxSpielfeldX),randomI(0,maxSpielfeldY));
            }
        }
    }

    private bool PlaceRoom(int art, int x, int y)
    {
        if (simpleMap[x][y][0] == 0)    //gewollter platz ist frei
        {
            if (art == bossRoom)        //gerade wird der bossroom platziert (-> treppe mit platzieren)
            {
                if (x == 0 || y == 0 || x == maxSpielfeldX || y == maxSpielfeldY)       //ist nicht am rand der map
                {
                    return false;
                }
                else 
                {
                    simpleMap[x][y][0] = art;                                   //bossraum platzieren
                    roomList.Add(new int[] { art, roomList.Count, x, y });

                    bool stairsFound = false;                                    //in welche richtung vom boss raum kommen treppen?
                    int tempRandom = randomI(0, 4);                         //random start für varianz
                    for (int i = 0;i< 4 && !stairsFound; i++)           //falls blockiert (bsp durch spawn raum -> drehen bis frei)
                    {
                        if (((tempRandom + i) % 4) + 1 == 1 && simpleMap[x][y - 1][0] == 0)         //oben versuchen
                        {
                            stairsFound = true;
                            simpleMap[x][y - 1][0] = stairsRoom;
                            roomList.Add(new int[] { stairsRoom, roomList.Count, x, y-1});
                        }
                        else if (((tempRandom + i) % 4) + 1 == 2 && simpleMap[x+1][y][0] == 0)      //rechts versuchen
                        {
                            stairsFound = true;
                            simpleMap[x+1][y][0] = stairsRoom;
                            roomList.Add(new int[] { stairsRoom, roomList.Count, x+1,y});
                        }
                        else if (((tempRandom + i) % 4) + 1 == 3 && simpleMap[x][y + 1][0] == 0)    //unten versuchen
                        {
                            stairsFound = true;
                            simpleMap[x][y + 1][0] = stairsRoom;
                            roomList.Add(new int[] { stairsRoom, roomList.Count, x, y + 1 });
                        }
                        else if (((tempRandom + i) % 4) + 1 == 4 && simpleMap[x-1][y][0] == 0)      //links versuchen
                        {
                            stairsFound = true;
                            simpleMap[x-1][y][0] = stairsRoom;
                            roomList.Add(new int[] { stairsRoom, roomList.Count, x-1, y });
                        }
                    }

                    return true;    //beenden um nicht doppelt ein zu tragen
                }
            }
            simpleMap[x][y][0] = art;       //bei anderen räumen nicht so streng -> platzieren
            roomList.Add(new int[] { art, roomList.Count, x, y });
            return true;
        }
        return false;      //neu versuchen
    }



    //destroy/createRoom



    //MST connection creation rules
        //boss-stairs   only
        //boss/stairs !- spawn
        //boss - other = max weight




    void Start()
    {
        GenerateMap();
    }

    // Später für load der raumevents bei nähe
    void Update()
    {
        
    }
}
