using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEngine;
// using UnityEngine.EventSystems;

public class WorldManager : MonoBehaviour
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
    // Player Player;
    public GameObject[] playerControllerObjects;
    Player[] playerControllers;

    public GameObject UIControllerObject;
    UIControllerScript uiController;

    public GameObject mapGeneratorObject;
    MapGenScript mapGenerator;

    public GameObject cursorBox;
    public GameObject selectionBox;

    public int minTownDistance;

    int mapWidth;
    int mapHeight;
    
    int currPlayer;
    int numPlayers = 2;

    int turnNumber;

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
        playerControllers = new Player[2];

        for (int i = 0; i < numPlayers; i++)
        {
            playerControllerObjects[i] = Instantiate(playerControllerPrefab);
            playerControllers[i] = playerControllerObjects[i].GetComponent<Player>();
            playerControllers[i].Initialize(i, this, uiController);
        }

        terrainGrid = new GameObject[mapWidth, mapHeight];
        featureGrid = new GameObject[mapWidth, mapHeight];
        mapGenerator.GenerateMap(mapWidth, mapHeight);

        currPlayer = 0;
        uiController.SetCurrPlayer(playerControllers[currPlayer]);

        turnNumber = 0;

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

        Select(playerControllers[currPlayer].getPlayerSelection(selected));

        if (endTurnPressed)
        {
            EndTurn();
        }
    }

    public void Select(GameObject target)
    {
        selected = target;
        if (selected != null)
        {
            selectionBox.SetActive(true);
            selectionBox.transform.position = new Vector3(selected.transform.position.x, selected.transform.position.y, -1.01f);
        }
        else
        {
            selectionBox.SetActive(false);
        }
    }

    public GameObject CreateStartingTown(int x, int y, int p)
    {
        GameObject t = SpawnTown(x, y, p);
        playerControllers[p].setMainTown(t);
        return t;
    }

    public bool CanBuildTown(Vector2Int location)
    {
        // since towns are built by units, location will always be occupied by a unit before building
        //if(unitGrid[location[0], location[1]] != null)
        //{
        //    Debug.Log("Space occupied by unit");
        //    return false;
        //}

        if (featureGrid[location[0], location[1]] != null)
        {
            Debug.Log("Space occupied by map feature");
            return false;
        }

        foreach (GameObject t in townList)
        {
            TownScript s = t.GetComponent<TownScript>();
            int distance = (Math.Abs(location[0] - s.xy()[0]) + Math.Abs(location[1] - s.xy()[1]));
            if (distance < minTownDistance)
            {
                Debug.Log("Too close to another town");
                return false;
            }
        }

        return true;
    }

    public void BuildTown()
    {
        // Causes the currently selected unit to build a town

        UnitScript s = selected.GetComponent<UnitScript>();

        int p = s.GetPlayer();
        Vector2Int xy = s.xy();

        if (s.isTownBuilder && CanBuildTown(xy))
        {
            DeleteUnit(selected);
            SpawnTown(xy.x, xy.y, p);
            // return SpawnTown(xy.x, xy.y, p);
        } else
        {
            // return null;
        }
    }

    public GameObject SpawnTown(int x, int y, int p)
    {
        if (unitGrid[x, y] == null)
        {
            int[] screencoords = MapToScreenCoordinates(x, y);

            GameObject newTown = Instantiate(strongholdPrefab, new Vector3(screencoords[0], screencoords[1], -1), Quaternion.identity);

            TownScript townScript = newTown.GetComponent<TownScript>();

            townScript.worldManagerObject = this.transform.gameObject;
            townScript.playerControllerObject = playerControllerObjects[p];
            townScript.uiControllerObject = UIControllerObject;

            townList.Add(newTown);

            unitGrid[x, y] = newTown;
            townScript.mapX = x;
            townScript.mapY = y;
            townScript.playerNumber = p;

            playerControllers[p].addTown(newTown);

            return newTown;
        }
        else
        {
            Debug.Log("Space occupied");
            return null;
        }
    }

    public GameObject SpawnPlayerUnit(GameObject unitPrefab, Player p)
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

        UnitScript uScript = u.GetComponent<UnitScript>();

        Vector2Int unitCoords = uScript.xy();

        unitGrid[uScript.mapX, uScript.mapY] = null;

        if (uScript.GetPlayer() >= 0)
        {
            playerControllers[uScript.GetPlayer()].deleteUnit(u);
        }

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

    public GameObject GetTerrainByElevation(int e)
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

    public int TownLimit()
    {
        return (int)Math.Floor(Math.Log(turnNumber, 3)) + 1;
    }

    public bool MoveUnit(GameObject unit, int x, int y)
    {
        if(!Walkable(x, y))
        {
            Debug.Log("Space not walkable");
            return false;
        }
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

                if (unitScript.GetPlayer() >= 0)    // hackish bypass for bug w/ neutral untis
                {
                    playerControllers[unitScript.GetPlayer()].CheckVision(x, y, unitScript.visionRange);
                }

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

    public void NextUnit()
    {
        playerControllers[currPlayer].SelectFirstUnitWithMoves();
    }

    public void EndTurnButtonPressed()
    {
        endTurnPressed = true;
    }

    public void StartTurn()
    {
        uiController.SetCurrPlayer(playerControllers[currPlayer]);

        if(currPlayer == 0)
        {
            turnNumber += 1;
        }

        playerControllers[currPlayer].StartTurn();
    }

    public void EndTurn()
    {
        endTurnPressed = false;
        selected = null;
        playerControllers[currPlayer].EndTurn();
        currPlayer = (currPlayer + 1) % numPlayers;

        if(currPlayer == 0)
        {
            MoveNeutralUnits();
        }

        StartTurn();
    }

    public int[] GetMapDimensions()
    {
        return new int[] {mapHeight, mapWidth };
    }

    public Player CheckForWinner()
    {
        Player lastActive = null;

        foreach(Player p in playerControllers)
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

    void MoveNeutralUnits()
    {
        foreach(GameObject u in unitList)
        {
            NeutralUnitScript s = u.GetComponent<NeutralUnitScript>();
            if(s != null)
            {
                s.AutoMove();
                s.ResetMovePoints();
            }
        }
    }
}
