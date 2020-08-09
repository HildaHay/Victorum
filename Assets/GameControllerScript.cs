using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEngine;
// using UnityEngine.EventSystems;

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

    public GameObject shrinePrefab;

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

    public GameObject mapGeneratorObject;
    MapGenScript mapGenerator;

    public GameObject cursorBox;
    public GameObject selectionBox;

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

        mapGenerator = mapGeneratorObject.GetComponent<MapGenScript>();

        playerControllerObjects = new GameObject[2];
        playerControllers = new PlayerControllerScript[2];

        for (int i = 0; i < numPlayers; i++)
        {
            playerControllerObjects[i] = Instantiate(playerControllerPrefab);
            playerControllers[i] = playerControllerObjects[i].GetComponent<PlayerControllerScript>();
            playerControllers[i].Initialize(i, this, uiController);
        }

        terrainGrid = new GameObject[mapWidth, mapHeight];
        featureGrid = new GameObject[mapWidth, mapHeight];
        mapGenerator.GenerateMap(mapWidth, mapHeight);

        currPlayer = 0;
        uiController.SetCurrPlayer(playerControllers[currPlayer]);

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
            cursorBox.transform.position = new Vector3(hit.transform.position.x, hit.transform.position.y, -1);
        }

        selected = playerControllers[currPlayer].getPlayerSelection(selected);
        if (selected != null)
        {
            selectionBox.SetActive(true);
            selectionBox.transform.position = new Vector3(selected.transform.position.x, selected.transform.position.y, -1.01f);
        } else
        {
            selectionBox.SetActive(false);
        }

        if(endTurnPressed)
        {
            EndTurn();
        }
    }

    public GameObject CreateStartingTown(int x, int y, int p)
    {
        GameObject t = SpawnTown(x, y, p);
        playerControllers[p].setMainTown(t);
        return t;
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

    public GameObject SpawnPlayerUnit(GameObject unitPrefab, PlayerControllerScript p)
    {
        GameObject newUnit = Instantiate(unitPrefab, new Vector3(0, 0, -1), Quaternion.identity);
        newUnit.GetComponent<UnitScript>().Initialize(p, this);

        unitList.Add(newUnit);

        p.addUnit(newUnit);

        return newUnit;
    }

    public GameObject SpawnUnit(GameObject unitPrefab, int x, int y)
    {
        GameObject newUnit = Instantiate(unitPrefab, new Vector3(0, 0, -1), Quaternion.identity);
        newUnit.GetComponent<UnitScript>().Initialize(this);

        unitList.Add(newUnit);
        
        if (newUnit != null)
        {
            unitList.Add(newUnit);
            unitGrid[x, y] = newUnit;
            newUnit.GetComponent<UnitScript>().mapX = x;
            newUnit.GetComponent<UnitScript>().mapY = y;

            int[] screenCoords = mapToScreenCoordinates(x, y);
            newUnit.transform.position = new Vector3(screenCoords[0], screenCoords[1], -1);
        }

        return newUnit;
    }

    //public GameObject SpawnNeutralUnit(GameObject unitPrefab)
    //{
    //    GameObject newUnit = Instantiate(unitPrefab, new Vector3(0, 0, -1), Quaternion.identity);
    //    newUnit.GetComponent<UnitScript>().Initialize(null, this);

    //    unitList.Add(newUnit);

    //    return newUnit;
    //}

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
            if (featureGrid[x, y].tag == "MapFeature")
            {
                return (terrainGrid[x, y].GetComponent<TileScript>().walkable && featureGrid[x, y].GetComponent<MapFeatureScript>().walkable);
            } else
            {
                return (terrainGrid[x, y].GetComponent<TileScript>().walkable);
            }
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

                playerControllers[unitScript.GetPlayer()].CheckVision(x, y, 3); // Remember: Replace the 3 with a variable!!!

                // uiController.ShowUnitInfo(unit);

                GameObject f = featureGrid[x, y];

                if (f != null)
                {
                    if (f.tag == "MapObjective")
                    {
                        for(int i = 0; i < playerControllers.Length; i++)
                        {
                            if(i == unitScript.GetPlayer())
                            {
                                playerControllers[i].ClaimShrine(f);
                            }
                            else
                            {
                                playerControllers[i].RemoveShrine(f);
                            }
                        }

                        // playerControllers[unitScript.GetPlayer()].ClaimShrine(featureGrid[x, y]);
                    }
                }

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
        uiController.SetCurrPlayer(playerControllers[currPlayer]);

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

    public int[] getMapDimensions()
    {
        return new int[] { mapWidth, mapHeight};
    }

    public int[] mapToScreenCoordinates(int x, int y)
    {
        int[] mapDimensions = GetMapDimensions();

        int a = x - mapDimensions[1] / 2;
        int b = -y + mapDimensions[0] / 2;

        return new int[] { a, b };
    }
}
