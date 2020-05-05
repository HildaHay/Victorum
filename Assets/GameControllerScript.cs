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
    public GameObject[,] unitGrid;

    // Prefabs

    // Terrain
    public GameObject grassTile;
    public GameObject sandTile;
    public GameObject dirtTile;
    public GameObject stoneTile;
    public GameObject waterTile;

    // Units
    public GameObject playerControllerPrefab;
    public GameObject knightPrefab;
    public GameObject strongholdPrefab;

    public List<GameObject> unitList;


    // Game Mechanics Objects
    // public GameObject PlayerController;
    // PlayerControllerScript playerControllerScript;
    public GameObject[] playerControllerObjects;
    PlayerControllerScript[] playerControllers;

    public GameObject UIControllerObject;
    UIControllerScript uiController;

    int mapWidth;
    int mapHeight;
    
    int currPlayer;
    int numPlayers = 2;

    bool endTurnPressed;

    bool startGame;

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

        selected = playerControllers[currPlayer].getPlayerSelection(selected);

        if(endTurnPressed)
        {
            EndTurn();
        }
    }

    public GameObject SpawnStronghold(int x, int y, int p)
    {
        if (unitGrid[x, y] == null)
        {
            int[] screencoords = MapToScreenCoordinates(x, y);

            GameObject newStronghold = Instantiate(strongholdPrefab, new Vector3(screencoords[0], screencoords[1], -1), Quaternion.identity);

            TownScript townScript = newStronghold.GetComponent<TownScript>();

            townScript.gameControllerObject = this.transform.gameObject;
            townScript.playerControllerObject = playerControllerObjects[p];
            townScript.uiControllerObject = UIControllerObject;

            unitList.Add(newStronghold);

            unitGrid[x, y] = newStronghold;
            townScript.mapX = x;
            townScript.mapY = y;
            townScript.player = p;

            playerControllers[p].addUnit(newStronghold);

            return newStronghold;
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
        newUnit.GetComponent<KnightScript>().Initialize(p, this);

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

        int[] unitCoords = u.GetComponent<KnightScript>().xy();

        print(unitCoords[0]);
        print(unitCoords[1]);

        print(unitGrid[unitCoords[0], unitCoords[1]]);

        KnightScript unitScript = u.GetComponent<KnightScript>();

        unitGrid[unitScript.mapX, unitScript.mapY] = null;

        playerControllers[u.GetComponent<KnightScript>().player].deleteUnit(u);

        Destroy(u);

        return true;
    }

    void GenerateMap(int w, int h)
    {
        /* int wOffset = w / 2;
        int hOffset = h / 2;

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                GameObject newTile;

                if (i == 0 || i == w-1 || j == 0 || j == h-1)
                {
                    newTile = Instantiate(waterTile, new Vector3(i - wOffset, -j + hOffset, 0), Quaternion.identity);
                }
                else
                {
                    int x = Random.Range(1, 4);
                    if (x == 1)
                    {
                        newTile = Instantiate(grassTile, new Vector3(i - wOffset, -j + hOffset, 0), Quaternion.identity);
                    }
                    else if (x == 2)
                    {
                        newTile = Instantiate(sandTile, new Vector3(i - wOffset, -j + hOffset, 0), Quaternion.identity);
                    }
                    else if (x == 3)
                    {
                        newTile = Instantiate(dirtTile, new Vector3(i - wOffset, -j + hOffset, 0), Quaternion.identity);
                    }
                    else
                    {
                        newTile = Instantiate(stoneTile, new Vector3(i - wOffset, -j + hOffset, 0), Quaternion.identity);
                    }
                }

                newTile.GetComponent<TileScript>().mapX = i;
                newTile.GetComponent<TileScript>().mapY = j;

                terrainGrid[i, j] = newTile;
            }
        }

        GameObject playerTown = SpawnStronghold(wOffset - 3, hOffset, 0);
        playerControllers[0].setMainTown(playerTown);
        GameObject enemyTown = SpawnStronghold(wOffset + 3, hOffset, 1);
        playerControllers[1].setMainTown(enemyTown); */

        Debug.Log("Generating terrain");

        // int mapsize = 31;
        // int mapcenter = mapsize / 2;
        int landsize = 500;

        int wOffset = w / 2;
        int hOffset = h / 2;

        int[][] map = new int[mapWidth][];
        for (int i = 0; i < mapWidth; i++)
        {
            map[i] = new int[mapHeight];
        }

        List<int[]> land = new List<int[]>();

        int[] landStart = { mapWidth / 2, mapHeight / 2 };

        land.Add(landStart);

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
                map[newLand[0]][newLand[1]] = 1;
                land.Add(newLand);
            }
        }

        Debug.Log("Terrain completed");

        string s = "";

        for(int i = 0; i < mapWidth; i++)
        {
            for(int j = 0; j < mapHeight; j++) {
                s = s + map[i][j].ToString();
            }
            s = s + "\n";
        }

        Debug.Log(s);

        terrainGrid = new GameObject[mapWidth, mapHeight];

        for(int i = 0; i < mapWidth; i++)
        {
            for(int j = 0; j < mapHeight; j++)
            {
                GameObject newTile;
                if(map[i][j] == 0)
                {
                    newTile = Instantiate(waterTile, new Vector3(i - wOffset, -j + hOffset, 0), Quaternion.identity);
                    terrainGrid[i,j] = newTile;
                } else
                {
                    newTile = Instantiate(grassTile, new Vector3(i - wOffset, -j + hOffset, 0), Quaternion.identity);
                }

                newTile.GetComponent<TileScript>().mapX = i;
                newTile.GetComponent<TileScript>().mapY = j;

                terrainGrid[i, j] = newTile;
            }
        }

        // create starting towns

        int r2 = UnityEngine.Random.Range(0, land.Count());
        int[] playerTownLocation = { land[r2][0], land[r2][1] };
        GameObject playerTown = SpawnStronghold(playerTownLocation[0], playerTownLocation[1], 0);
        playerControllers[0].setMainTown(playerTown);

        r2 = UnityEngine.Random.Range(0, land.Count());
        int[] enemyTownLocation = { land[r2][0], land[r2][1] };
        GameObject enemyTown = SpawnStronghold(enemyTownLocation[0], enemyTownLocation[1], 1);
        playerControllers[1].setMainTown(enemyTown);
    }

    public int[] MapToScreenCoordinates(int x, int y)
    {
        int a = x - mapWidth / 2;
        int b = y - mapHeight / 2;

        return new int[] { x - mapWidth / 2, -y + mapHeight / 2};
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

            KnightScript unitScript = unit.GetComponent<KnightScript>();

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
}
