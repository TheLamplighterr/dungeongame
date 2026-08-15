using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public TreeBranch Quadtree;
    public RoomCollection roomCollection;

    private int maxSpielfeldX = 7;
    private int maxSpielfeldY = 7;

    //�bersichtsvariablen
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
    public int poiCount = 2;            //let       Points of interrest (ereignissr�ume)
    public int treasureRoomCount = 1;   //let       
    public int bossRoomCount = 1;       //const

    public int level = 1;

    private int bossToStairsDir;

    public int GetBossToStairsDir()
    {
        return bossToStairsDir;
    }

    public int getLevel()
    {
        return level;
    }

    public MapManager GetMapManager()
    {
        return this;
    }

    public TreeBranch GetQuadTree()
    {
        return this.Quadtree;
    }

    public RoomCollection GetRoomCollection() 
    { 
        return roomCollection;
    }

    //level boundaries
    //adjustGenerationBoundaries

    public static float GetMapScale()
    {
        return scale;
    }
    public static float GetMapDistance() { return distance; }

    //roomInitRequest
    public int deliverSimplePathFromXY(int givenX, int givenY)
    {
        return simpleMap[givenX][givenY][5];
    }

    //deliver room openings
    public int[] deliverRoomOpeningsForXY(int givenX, int givenY)
    {
        return new int[4] {simpleMap[givenX][givenY][1], simpleMap[givenX][givenY][2], 
                            simpleMap[givenX][givenY][3], simpleMap[givenX][givenY][4]};
    }

    List<int[]> roomList = new List<int[]>()        //liste aller aktiven r�ume mit position und verweis
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
            return new int[] { roomList[givenID][2], roomList[givenID][3]};     //gib den x und y wert zur�ck
        }
        else
        {
            return new int[] { -5, -5 };            //sonst default zu -5/-5 (au�erhalb der map)
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
    //[x][y][5] =   Placeholder (zus�tzliche Eigenschaften)     //placeholder f�r leichteren quad tree verweis? (bsp: 143 -> In bereich 1 von bereich 4 von bereich 3)
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
        return (int) UnityEngine.Random.Range(x, y + 1);
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
        prepareConnections();

        for (int i = 0; i < connections.Count; i++)
        {
            Debug.Log("Verbindung "+ i + ": Weight = <" + connections[i][0] + "> ConnectedPoints: " + connections[i][1] + " and " + connections[i][2]);
        }

        //connect rooms         (try to create a path for each connection)

        createConnectionPaths();


        //translateToQuadTree   (quadtree leaves mit r�umen f�llen)
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
                    int tempRandom = randomI(0, 4);                         //random start f�r varianz
                    for (int i = 0;i< 4 && !stairsFound; i++)           //falls blockiert (bsp durch spawn raum -> drehen bis frei)
                    {
                        if (((tempRandom + i) % 4) + 1 == 1 && simpleMap[x][y - 1][0] == 0)         //oben versuchen
                        {
                            stairsFound = true;
                            simpleMap[x][y - 1][0] = stairsRoom;
                            spawnField[0] = x;
                            spawnField[1] = y-1;
                            roomList.Add(new int[] { stairsRoom, roomList.Count, x, y-1});
                            bossToStairsDir = 1;
                        }
                        else if (((tempRandom + i) % 4) + 1 == 2 && simpleMap[x+1][y][0] == 0)      //rechts versuchen
                        {
                            stairsFound = true;
                            simpleMap[x+1][y][0] = stairsRoom;
                            spawnField[0] = x+1;
                            spawnField[1] = y;
                            roomList.Add(new int[] { stairsRoom, roomList.Count, x+1,y});
                            bossToStairsDir = 2;
                        }
                        else if (((tempRandom + i) % 4) + 1 == 3 && simpleMap[x][y + 1][0] == 0)    //unten versuchen
                        {
                            stairsFound = true;
                            simpleMap[x][y + 1][0] = stairsRoom;
                            spawnField[0] = x;
                            spawnField[1] = y + 1;
                            roomList.Add(new int[] { stairsRoom, roomList.Count, x, y + 1 });
                            bossToStairsDir = 3;
                        }
                        else if (((tempRandom + i) % 4) + 1 == 4 && simpleMap[x-1][y][0] == 0)      //links versuchen
                        {
                            stairsFound = true;
                            simpleMap[x-1][y][0] = stairsRoom;
                            spawnField[0] = x - 1;
                            spawnField[1] = y;
                            roomList.Add(new int[] { stairsRoom, roomList.Count, x-1, y });
                            bossToStairsDir = 4;
                        }
                    }

                    return true;    //beenden um nicht doppelt ein zu tragen
                }
            }
            simpleMap[x][y][0] = art;       //bei anderen r�umen nicht so streng -> platzieren
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

    List<int[]> connections = new List<int[]>();
    //connection form: weight, p1, p2

    List<int[]> families = new List<int[]>();
    //familyForm: roomListID, famInt


    private void resetLists()
    {
        connections = new List<int[]>();
        families = new List<int[]>();
        

    }


    int calculateWeightFromXY(int x, int y, int x2, int y2)
    {
        int result = 0;
        int temp = x - x2;
        if (temp < 0) { temp = temp * -1; }
        result = temp;
        temp = y - y2;
        if (temp < 0) { temp = temp * -1; }
        result = result + temp;
        Debug.Log("Calculated <" + result + "> as weight of connection: X"+x + " Y"+y + " and X"+x2 + "Y"+y2);
        return result;
    }

    void prepareFamilies()
    {
        families = new List<int[]>();
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i][listRoomType] != bossRoom && roomList[i][listRoomType] != stairsRoom)
            {
                families.Add(new int[2] { roomList[i][listRoomID], 0 });
            }
        }
    }       

    void prepareConnections()
    {
        Debug.Log("Preparing MST-Connections");
        prepareFamilies();      //f�r jeden zu beachtenden Raum wird ein punkt mit Familie und Verweis erstellt

        List<int[]> rawConnections = new List<int[]>();
        for (int i = 0; i < families.Count; i++)    //alle Punkte sollen versuchen sich mit allen Punkte zu verbinden
        {
                for (int j = 0; j < families.Count; j++)  //alle Punkte werden mit allen Punkten verbunden
                { 
                    if(i != j) //  ein Punkt soll sich nicht mit sich selbst verbinden
                    {
                        rawConnections.Add(new int[3]
                        {   //    \/ aus X und Y der 2 Punkte wird ein Gewicht berechnet
                            calculateWeightFromXY(roomList[i][listRoomX], roomList[i][listRoomY], roomList[j][listRoomX],roomList[j][listRoomY]),
                            i,          //Punkt 1
                            j           //Punkt 2
                        }
                        );
                    }
                }
        }
        
        rawConnections = filterConnections(rawConnections); //doppelte Verbindungen werden entfernt

        rawConnections = sortConnectionsByWeight(rawConnections);   //Verbindungen werden der Gr��e nach sortiert

        int familyCount = 0;

        for (int i = 0; i < rawConnections.Count; i++)  //versuche alle rohen Verbindungen zu best�tigen
        {

            if (families[rawConnections[i][1]][1] == 0 && families[rawConnections[i][2]][1] == 0)   //beide Punkte ohne Familie -> neue erstellen
            {
                familyCount++;              
                families[rawConnections[i][1]][1] = familyCount;
                families[rawConnections[i][2]][1] = familyCount;

                connections.Add(rawConnections[i]);
            }
            else if (families[rawConnections[i][1]][1] == families[rawConnections[i][2]][1])        //filter: gleiche Familie = schon verbunden -> skip
            {
                //wenn sie bereits verbunden sind abbrechen
            }
            else if (families[rawConnections[i][1]][1] != 0 && families[rawConnections[i][2]][1] == 0)  //1 hat Familie, 2 nicht: 1 <- 2
            {
                families[rawConnections[i][2]][1] = families[rawConnections[i][1]][1];

                connections.Add(rawConnections[i]);
            }
            else if (families[rawConnections[i][1]][1] == 0 && families[rawConnections[i][2]][1] != 0)  //2 hat Familie, 1 nicht: 2 <- 1
            {
                families[rawConnections[i][1]][1] = families[rawConnections[i][2]][1];

                connections.Add(rawConnections[i]);
            }
            else if (families[rawConnections[i][1]][1] != 0 && families[rawConnections[i][2]][1] != 0)  //beide haben Familie: merge 1 <- 2
            {
                mergeFamily(families[rawConnections[i][2]][1], families[rawConnections[i][1]][1]);

                connections.Add(rawConnections[i]);
            }
        }





    }

    void mergeFamily(int pre, int post)
    {
        for (int i = 0; i < families.Count; i++)
        {
            if(families[i][1] == pre)
            {
                families[i][1] = post;
            }
        }
    }

    List<int[]> sortConnectionsByWeight(List<int[]> given)
    {
        List<int[]> result = given;
        result.Sort((a, b) => a[0] - b[0]);
        return result;
    }

    List<int[]> filterConnections(List<int[]> given)
    {
        List<int[]> result = new List<int[]>();

        for (int i = 0; i < given.Count; i++) 
        {
            if(!connectionIsDouble(result, given[i][1], given[i][2]))
            {
                    result.Add(given[i]);
            }
        }

        return result;
    }

    bool connectionIsDouble(List<int[]> given, int p1, int p2)
    {
        for (int i = 0; i < given.Count; i++)
        {
                if ((given[i][1] == p1 && given[i][2] == p2) || (given[i][1] == p2 && given[i][2] == p1))
                {
                    return true;
                }
            }
            return false;
    }

    /*Connections: weight families[][RoomlistID,familyNR]
     * connection -> family -> id -> coords
    */
    void createConnectionPaths()
    {
        for (int i = 0; i < connections.Count; i++) 
        {
            createSingleConnectionPath(families[connections[i][1]][0], families[connections[i][2]][0], false);
        }

        //Bossroom connect
        //find boss room
        int bossRoomID = 0;
        int bossConnectorID = 0;
        int weightToBeat = 100;

        for (int i = 0;i < roomList.Count; i++)                 
        {
            if (roomList[i][listRoomType] == bossRoom)
            {
                bossRoomID = i;
            }
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            if(roomList[i][listRoomType] == poiRoom || roomList[i][listRoomType] == treasureRoom)
            {
                if(calculateWeightFromXY(roomList[bossRoomID][listRoomX], roomList[bossRoomID][listRoomY], roomList[i][listRoomX], roomList[i][listRoomY]) <= weightToBeat)
                {
                    bossConnectorID = i;
                }
            }
        }
        createSingleConnectionPath(bossConnectorID, bossRoomID,true);

        //boss to healroomConnect

    }

    void createSingleConnectionPath(int id1, int id2, bool ignoreRestriction)
    {
        int[] point1 = new int[2] { roomList[id1][listRoomX], roomList[id1][listRoomY] };
        int[] point2 = new int[2] { roomList[id2][listRoomX], roomList[id2][listRoomY] };
        int[] currentPos = point1;


        //int attempt = 0;                        //deadends reached
        //bool decisionNeeded = false;            //collision detected, multiple options
        bool deadend = false;                   //temp var to end while
        bool destination = false;
        int lastMoove = 0;                      //0=none 1=up 2=right 3=down 4=left

        List<int[]> attempt1 = new List<int[]>();
        List<int[]> attempt2 = new List<int[]>();


        while (deadend == false && destination == false) 
        {
            if (currentPos[0] == point2[0] && currentPos[1] == point2[1])
            {
                destination = true;
            }
            else
            {
                int tempdir = calculateNextMoove(point1[0], point1[1], point2[0], point2[1], false, false, lastMoove);

                attempt1.Add(new int[3] { currentPos[0], currentPos[1], tempdir });

                //lastMoove = tempdir;

                if(tempdir == 1)
                {
                    currentPos[1]--;
                }
                else if (tempdir == 2)
                {
                    currentPos[0]++;
                }
                else if (tempdir == 3)
                {
                    currentPos[1]++;
                }
                else if (tempdir == 4)
                {
                    currentPos[0]--;
                }
            }

        }

        for (int i = 0; i < attempt1.Count; i++) 
        {
            applyConnectionPath(attempt1[i][0], attempt1[i][1], attempt1[i][2]);
        }


    }

    int calculateNextMoove(int X1, int Y1, int X2, int Y2, bool restricted, bool try2, int lastdir)     //Moo :p
    {
        int distanceX = X1 - X2;                            
        if(distanceX < 0) distanceX = distanceX * -1;       //get x distance
        int distanceY = Y1 - Y2;
        if(distanceY < 0)distanceY = distanceY * -1;        //get y distance

        int Xdir = 0;                                       //
        if(X1 > X2) {  Xdir = -1; }
        if (X1 < X2) { Xdir = 1; }
        int Ydir = 0;
        if(Y1 > Y2) { Ydir = -1; }
        if (Y1 < Y2) { Ydir = 1; }

        int result = 0;

        if (distanceX > distanceY)
        {
            result = 2;
            if (Xdir == -1)
            {
                result = 4;
            }
            if (mooveIsValid(X1, Y1, result, restricted))
            {
                return result;
            }
        }
        else if(distanceX < distanceY)
        {
            result = 3;
            if (Ydir == -1) 
            {
                result = 1;
            }
            if (mooveIsValid(X1, Y1, result, restricted))
            {
                return result;
            }
            
        }

        if(distanceX == distanceY)
        {
            if(try2 == true)
            {
                result = 3;
                if (Ydir == -1)
                {
                    result = 1;
                }
                if (mooveIsValid(X1, Y1, result, restricted))
                {
                    return result;
                }

            }
            else
            {
                result = 2;
                if (Xdir == -1)
                {
                    result = 4;
                }
                if (mooveIsValid(X1, Y1, result, restricted))
                {
                    return result;
                }
            }
        }

        if (mooveIsValid(X1, Y1, result, restricted))
        {
            return result;
        }
        

        return 0;
    }
    

    

    bool mooveIsValid(int X, int Y, int dir, bool restricted) 
    {
        return true;
        /*
        if (dir == 1)
        {
            if(Y-1 <= 0) {  return false; }
            int temp = simpleMap[X][Y - 1][listRoomType];
            if (temp != stairsRoom)
            {
                if (!restricted || temp != bossRoom)
                    { return true; }
            }
        }
        else if (dir == 2)
        {
            if (X + 1 >= maxSpielfeldX) { return false; }
            int temp = simpleMap[X + 1][Y][listRoomType];
            if (temp != stairsRoom)
            {
                if (!restricted || temp != bossRoom)
                { return true; }
            }
        }
        else if (dir == 3)
        {
            if (Y + 1 >= maxSpielfeldY) { return false; }
            int temp = simpleMap[X][Y + 1][listRoomType];
            if (temp != stairsRoom)
            {
                if (!restricted || temp != bossRoom)
                { return true; }
            }
        }
        else if (dir == 4)
        {
            if (X - 1 <= 0) { return false; }
            int temp = simpleMap[X - 1][Y][listRoomType];
            if (temp != stairsRoom)
            {
                if (!restricted || temp != bossRoom)
                { return true; }
            }
        }*/
            //return false;
    }

    void applyConnectionPath(int X, int Y, int dir)
    {
        if (simpleMap[X][Y][0] == 0)
        {
            roomList.Add(new int[4] { connectorRoom, roomList.Count, X, Y });
            simpleMap[X][Y][0] = connectorRoom;
        }

        if(dir == 1)
        {
            simpleMap[X][Y][1] = 1;
            simpleMap[X][Y - 1][3] = 1;
        }
        else if (dir == 2)
        {
            simpleMap[X][Y][2] = 1;
            simpleMap[X+1][Y][4] = 1;
        }
        else if (dir == 3)
        {
            simpleMap[X][Y][3] = 1;
            simpleMap[X][Y+1][1] = 1;
        }
        else if (dir == 4)
        {
            simpleMap[X][Y][4] = 1;
            simpleMap[X-1][Y][2] = 1;
        }

    }

    /*
    each point: int family (id compared point) 
    int[roomNR] families {familyID} (base -1 if no family)

    int [] possibleConnections {0,1} calculate weight by distance, sort by weight
    

    families [familyID][connections]

    look at possible connection(allready sorted by weight):
    point 0: getFamily 
    point 1: getFamily
    if family is same -> cancel, moove on
    else all in families where family == point 1 family = point 0 family
 
    repeat for all sorted possible connections


    make algorythm to follow connection (try straight, zigzag random if not straight(unless obstructed: in that case try to moove arround the object))

    look for shortest connection with longest path for boss room


    save path for a bit (save as array, maybe dir) -> if path works -> create rooms, edit connection saves)

    quadtree apply openings


    */

    void easyFixToEndMySuffering()
    {
        foreach(RoomInit ri in FindObjectsOfType<RoomInit>())
            ri.destroySelf();
    }

    public void generateNewLevel()
    {
        level++;

        resetLists();
        easyFixToEndMySuffering();
        if(Quadtree.destroyAllLinkedRooms() == 1)
        {
            generateSimpleMap();
        }
    }

    void Start()
    {
        GenerateMap();
    }

    // Sp�ter f�r load der raumevents bei n�he
    void Update()
    {
        
    }
    
    //Signalisiert den Tod des Bossgegners
    public void BossDefeat()
    {
        BossRoomInit bossRoomInit = FindObjectOfType<BossRoomInit>();
        if (bossRoomInit != null) 
        { 
            bossRoomInit.bossDefeat();
        }
    }
}
