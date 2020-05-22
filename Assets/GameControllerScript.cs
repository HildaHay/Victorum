using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameControllerScript : MonoBehaviour
{

    GameObject selected;

    public Camera mainCamera;

    public GameObject[,] terrainGrid;
    public GameObject[,] featureGrid;
    public GameObject[,] unitGrid;

    // Prefabs

    // Terrain
    public GameObject grassTile;
    public GameObject sandTile;
    public GameObject dirtTile;
    public GameObject stoneTile;
    public GameObject waterTile;

    public GameObject treeFeature;

    public int[] tileWeights;

    // Units
    public GameObject playerControllerPrefab;
    public GameObject strongholdPrefab;

    public List<GameObject> unitList;
    public List<GameObject> townList;   // could possibly be combined with unitList


    // Game Mechanics Objects
    // public GameObject PlayerController;
    // PlayerControllerScript playerControllerScript;
    public GameObject[] playerControllerObjects;
    PlayerControllerScript[] playerControllers;

    public GameObject UIControllerObject;
    UIControllerScript uiController;

    public GameObject highlightBox;

    int mapWidth;
    int mapHeight;
    
    int currPlayer;
    int numPlayers = 2;

    bool endTurnPressed;

    bool startGame;

    public bool gameOver;

    // Start is called before the first frame update
    void Start()
    {
        // mapWidth = 17;
        // mapHeight = 11;

        mapWidth = 51;
        mapHeight = 51;

        mainCamera = Camera.main;

        // terrainGrid = new GameObject[mapWidth, mapHeight];
        unitGrid = new GameObject[mapWidth, mapHeight];

        uiController = UIControllerObject.GetComponent<UIControllerScript>();

        playerControllerObjects = new GameObject[2];
        playerControllers = new PlayerControllerScript[2];

        for (int i = 0; i < numPlayers; i++)
        {
            playerControllerObjects[i] = Instantiate(playerControllerPrefab);
            playerControllers[i] = playerControllerObjects[i].GetComponent<PlayerControllerScript>();
            playerControllers[i].Initialize(i, this, uiController);
        }

        GenerateMap(mapWidth, mapHeight);

        currPlayer = 0;
        uiController.SetCurrPlayer(currPlayer);

        endTurnPressed = false;

        gameOver = false;

        startGame = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(startGame == true)
        {
            startGame = false;
            StartTurn();
        }

        if(Input.GetKeyDown("up"))
        {
            mainCamera.transform.position += new Vector3(0, 1, 0);
        }
        if (Input.GetKeyDown("down"))
        {
            mainCamera.transform.position += new Vector3(0, -1, 0);
        }
        if (Input.GetKeyDown("right"))
        {
            mainCamera.transform.position += new Vector3(1, 0, 0);
        }
        if (Input.GetKeyDown("left"))
        {
            mainCamera.transform.position += new Vector3(-1, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            if (mainCamera.orthographicSize < 100)
            {
                mainCamera.orthographicSize += 1;
            }
        }
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            if (mainCamera.orthographicSize > 1)
            {
                mainCamera.orthographicSize -= 1;
            }
        }
        if (Input.GetKeyDown(KeyCode.Home))
        {
            mainCamera.orthographicSize = 7;
            
        }

        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), new Vector3(0, 0, 1));

        if (hit) {
            highlightBox.transform.position = new Vector3(hit.transform.position.x, hit.transform.position.y, -1);
        }

        selected = playerControllers[currPlayer].getPlayerSelection(selected);

        if(endTurnPressed)
        {
            EndTurn();
        }
    }

    public GameObject SpawnTown(int x, int y, int p)
    {
        if (unitGrid[x, y] == null)
        {
            int[] screencoords = MapToScreenCoordinates(x, y);

            GameObject newTown = Instantiate(strongholdPrefab, new Vector3(screencoords[0], screencoords[1], -1), Quaternion.identity);

            TownScript townScript = newTown.GetComponent<TownScript>();

            townScript.gameControllerObject = this.transform.gameObject;
            townScript.playerControllerObject = playerControllerObjects[p];
            townScript.uiControllerObject = UIControllerObject;

            townList.Add(newTown);

            unitGrid[x, y] = newTown;
            townScript.mapX = x;
            townScript.mapY = y;
            townScript.player = p;

            playerControllers[p].addTown(newTown);

            return newTown;
        }
        else
        {
            Debug.Log("Space occupied");
            return null;
        }
    }

    public GameObject SpawnUnit(GameObject unitPrefab, int p)
    {
        GameObject newUnit = Instantiate(unitPrefab, new Vector3(0, 0, -1), Quaternion.identity);
        newUnit.GetComponent<UnitScript>().Initialize(p, this);

        unitList.Add(newUnit);

        playerControllers[p].addUnit(newUnit);

        return newUnit;
    }

    public bool DeleteUnit(GameObject u)
    {
        if(!unitList.Remove(u)) {
            Debug.Log("Error: failed to delete unit from list");
            return false;
        }

        int[] unitCoords = u.GetComponent<UnitScript>().xy();

        print(unitCoords[0]);
        print(unitCoords[1]);

        print(unitGrid[unitCoords[0], unitCoords[1]]);

        UnitScript unitScript = u.GetComponent<UnitScript>();

        unitGrid[unitScript.mapX, unitScript.mapY] = null;

        playerControllers[u.GetComponent<UnitScript>().GetPlayer()].deleteUnit(u);

        Destroy(u);

        return true;
    }

    public bool DeleteTown(GameObject t)    // could possibly be combined with deleteUnit
    {
        if (!townList.Remove(t))
        {
            Debug.Log("Error: failed to delete unit from list");
            return false;
        }

        int[] townCoords = t.GetComponent<TownScript>().xy();

        print(townCoords[0]);
        print(townCoords[1]);

        print(unitGrid[townCoords[0], townCoords[1]]);

        TownScript unitScript = t.GetComponent<TownScript>();

        unitGrid[unitScript.mapX, unitScript.mapY] = null;

        playerControllers[t.GetComponent<TownScript>().GetPlayer()].deleteTown(t);

        Destroy(t);

        return true;
    }

    void GenerateMap(int w, int h)
    {
        

        Debug.Log("Generating terrain");

        int landsize = 650;
        // int landsize = 50;

        int wOffset = w / 2;
        int hOffset = h / 2;

        //Syl Note : Why are you using C style code here? Why not Use a 2d vector or list?
        int[][] map = new int[mapWidth][];
        for (int i = 0; i < mapWidth; i++)
        {
            map[i] = new int[mapHeight];
        }

        List<int[]> land = new List<int[]>();

        int[] landStart = { mapWidth / 2, mapHeight / 2 };

        land.Add(landStart);

        int maxElevation = 0;

        while(land.Count < landsize)
        {

            // Pick a random tile to start at

            int r = UnityEngine.Random.Range(0, land.Count());
            int[] newLand = { land[r][0], land[r][1] };

            // Move one tile in a random direction
            // WARNING: there is nothing to stop this from going off the map!
            // This will be fixed later but for now just hope it doesn't happen
            int dir = UnityEngine.Random.Range(0, 4);
            switch (dir)
            {
                case 0: // shift up
                    newLand[0] -= 1;
                    break;
                case 1: // shift down
                    newLand[0] += 1;
                    break;
                case 2: // shift left
                    newLand[1] -= 1;
                    break;
                default: // shift right
                    newLand[1] += 1;
                    break;
            }

            if(map[newLand[0]][newLand[1]] == 0)
            {
                // change from water to land
                land.Add(newLand);
                map[newLand[0]][newLand[1]] = 1;
            } else {
                // increase elevation by 1
                map[newLand[0]][newLand[1]] += 1;
                if(map[newLand[0]][newLand[1]] > maxElevation)
                {
                    maxElevation = map[newLand[0]][newLand[1]];
                }
            }
        }

        Debug.Log(maxElevation);
        Debug.Log("Terrain completed");

        /*
        string s = "";

        for(int i = 0; i < mapWidth; i++)
        {
            for(int j = 0; j < mapHeight; j++) {
                s = s + map[i][j].ToString();
            }
            s = s + "\n";
        }

        Debug.Log(s);
        */

        terrainGrid = new GameObject[mapWidth, mapHeight];
        featureGrid = new GameObject[mapWidth, mapHeight];

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                GameObject newTile = Instantiate(getTerrainByElevation(map[i][j]), new Vector3(i - wOffset, -j + hOffset, 0), Quaternion.identity);

                newTile.GetComponent<TileScript>().mapX = i;
                newTile.GetComponent<TileScript>().mapY = j;

                terrainGrid[i, j] = newTile;

                if (map[i][j] != 0 && UnityEngine.Random.Range(0, 10) == 0)
                {
                    GameObject newFeature = Instantiate(treeFeature, new Vector3(i - wOffset, -j + hOffset, -1), Quaternion.identity);

                    featureGrid[i, j] = newFeature;
                }
            }
        }

        // create starting towns

        int r2 = UnityEngine.Random.Range(0, land.Count());
        int[] playerTownLocation = { land[r2][0], land[r2][1] };
        GameObject playerTown = SpawnTown(playerTownLocation[0], playerTownLocation[1], 0);
        playerControllers[0].setMainTown(playerTown);

        r2 = UnityEngine.Random.Range(0, land.Count());
        int[] enemyTownLocation = { land[r2][0], land[r2][1] };
        GameObject enemyTown = SpawnTown(enemyTownLocation[0], enemyTownLocation[1], 1);
        playerControllers[1].setMainTown(enemyTown);
    }

    public int[] MapToScreenCoordinates(int x, int y)
    {
        int a = x - mapWidth / 2;
        int b = y - mapHeight / 2;

        return new int[] { x - mapWidth / 2, -y + mapHeight / 2};
    }

    public bool Walkable(int x, int y)
    {
        if(featureGrid[x, y] == null)
        {
            return (terrainGrid[x, y].GetComponent<TileScript>().walkable);
        } else { 
            return (terrainGrid[x, y].GetComponent<TileScript>().walkable && featureGrid[x, y].GetComponent<MapFeatureScript>().walkable);
        }
        // also should check if unit is in tile
    }

    public GameObject getTerrainByID(int id)
    {
        // Syl Note : Consider using an Enum, also possibly have tile rendiering be done by a single object or shader.
        switch (id)
        {
            case 0:
                return waterTile;
            case 1:
                return grassTile;
            case 2:
                return sandTile;
            case 3:
                return dirtTile;
            case 4:
                return stoneTile;
            default:
                return waterTile;
        }
    }

    public GameObject getTerrainByElevation(int e)
    {
        if(e == 0)
        {
            return waterTile;
        } else if(e < 5)
        {
            return sandTile;
        } else if(e < 20)
        {
            return grassTile;
        } else
        {
            return stoneTile;
        }
    }

    public bool MoveUnit(GameObject unit, int x, int y)
    {
        if (unitGrid[x, y] != null)
        {
            Debug.Log("Space occupied");
            return false;
        }
        else
        {

            UnitScript unitScript = unit.GetComponent<UnitScript>();

            int x_diff = Mathf.Abs(unitScript.mapX - x);
            int y_diff = Mathf.Abs(unitScript.mapY - y);

            if (unitScript.TryMove(x_diff + y_diff))
            {
                unitGrid[unitScript.mapX, unitScript.mapY] = null;
                unitGrid[x, y] = unit;

                unitScript.mapX = x;
                unitScript.mapY = y;

                int[] newScreenCords = MapToScreenCoordinates(x, y);
                unit.transform.position = new Vector3(newScreenCords[0], newScreenCords[1], -1);

                // uiController.ShowUnitInfo(unit);

                return true;
            }
            else
            {
                // uiController.ShowUnitInfo(unit);
                Debug.Log("Out of movement");
                
                return false;
            }
        }
    }

    public void EndTurnButtonPressed()
    {
        endTurnPressed = true;
    }

    public void StartTurn()
    {
        uiController.SetCurrPlayer(currPlayer);

        playerControllers[currPlayer].StartTurn();
    }

    public void EndTurn()
    {
        endTurnPressed = false;
        selected = null;
        playerControllers[currPlayer].EndTurn();
        currPlayer = (currPlayer + 1) % numPlayers;

        StartTurn();
    }

    public int[] GetMapDimensions()
    {
        return new int[] {mapHeight, mapWidth };
    }

    public PlayerControllerScript CheckForWinner()
    {
        PlayerControllerScript lastActive = null;

        foreach(PlayerControllerScript p in playerControllers)
        {
            if(p.playerActive)
            {
                if(lastActive == null)
                {
                    lastActive = p;
                } else
                {
                    return null;
                }
            }


        }

        if(lastActive != null)
        {
            gameOver = true;
            uiController.SetWinner(lastActive.playerNumber);
        }

        return null;
    }
}
