﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
// using UnityEngine.EventSystems;

public class WorldManager : MonoBehaviour
{
    // Debug Options
    [SerializeField] public bool debugMode;
    [SerializeField] public bool debugNoFoW;
    [SerializeField] public bool debugAutoPlay;
    float debugTurnTimer;

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

    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject strongholdPrefab;
    [SerializeField] public GameObject humanPlayerControllerPrefab;
    [SerializeField] public GameObject aiPlayerControllerPrefab;
    [SerializeField] public GameObject techTreePrefab;

    public List<GameObject> unitList;
    public List<GameObject> townList;   // could possibly be combined with unitList


    // Game Mechanics Objects
    // public GameObject PlayerController;
    // Player Player;
    public GameObject[] playerObjects;
    Player[] players;

    public GameObject UIControllerObject;
    UIManager uiController;

    public GameObject mapGeneratorObject;
    MapGenerator mapGenerator;

    public GameObject cursorBox;
    public GameObject selectionBox;

    public int minTownDistance;

    int mapWidth;
    int mapHeight;

    public int currPlayer;
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

        debugTurnTimer = 10.0f;

        // These should always be divisible by 5
        mapWidth = 50;
        mapHeight = 50;

        mainCamera = Camera.main;

        // terrainGrid = new GameObject[mapWidth, mapHeight];
        unitGrid = new GameObject[mapWidth, mapHeight];

        uiController = UIControllerObject.GetComponent<UIManager>();

        mapGenerator = mapGeneratorObject.GetComponent<MapGenerator>();

        playerObjects = new GameObject[2];
        players = new Player[2];

        for (int i = 0; i < numPlayers; i++)
        {
            playerObjects[i] = Instantiate(playerPrefab);
            players[i] = playerObjects[i].GetComponent<Player>();
            if (i == 0)
            {
                players[i].Initialize(i, this, uiController, true);
            }
            else
            {
                players[i].Initialize(i, this, uiController, false);
            }
        }

        terrainGrid = new GameObject[mapWidth, mapHeight];
        featureGrid = new GameObject[mapWidth, mapHeight];
        bool mapgenSuccess = mapGenerator.GenerateMap(mapWidth, mapHeight);
        if(!mapgenSuccess)
        {
            Debug.Log("Failed to generate map.");
            Application.Quit();
        }

        currPlayer = 0;
        uiController.SetCurrPlayer(players[currPlayer]);

        turnNumber = 0;

        endTurnPressed = false;

        gameOver = false;

        startGame = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (startGame == true)
        {
            startGame = false;
            StartTurn();
        }

        if (Input.GetKeyDown("up"))
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

        if (hit)
        {
            cursorBox.transform.position = new Vector3(hit.transform.position.x, hit.transform.position.y, -1);
            if(hit.transform.gameObject.tag == "Terrain")
            {
                TileScript tile = hit.transform.gameObject.GetComponent<TileScript>();
                uiController.SetCoordinatesText(new Vector2Int(tile.mapX, tile.mapY));
            }
        }

        // if (players[currPlayer].controller.IsHuman()) {
        //     Select(players[currPlayer].controller.getPlayerSelection(selected));
        // }

        if (debugAutoPlay)
        {
            debugTurnTimer -= Time.deltaTime;
            if (debugTurnTimer <= 0)
            {
                endTurnPressed = true;
                debugTurnTimer = 0.4f;
            }
        }

        if (endTurnPressed)
        {
            EndTurn();
        }
    }

    public void Select(GameObject target)
    {
        Deselect();
        selected = target;
        if (selected != null)
        {
            selectionBox.SetActive(true);
            selectionBox.transform.position = new Vector3(selected.transform.position.x, selected.transform.position.y, -1.01f);

            if (selected.tag == "Unit")
            {
                selected.GetComponent<UnitScript>().DrawPath();
            }
        }
        else
        {
            selectionBox.SetActive(false);
        }
    }

    public GameObject CreateStartingTown(int x, int y, int p)
    {
        GameObject t = SpawnTown(x, y, p);
        players[p].SetMainTown(t);
        return t;
    }

    public void Deselect()
    {
        if (selected != null)
        {
            if (selected.tag == "Unit")
            {
                selected.GetComponent<UnitScript>().ClearPath();
            }
        }
        selected = null;
    }

    public bool CanBuildTown(Vector2Int location)
    {
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
            GameObject temp = selected;
            Deselect();
            DeleteUnit(temp);
            SpawnTown(xy.x, xy.y, p);
            // return SpawnTown(xy.x, xy.y, p);
        }
        else
        {
            // return null;
        }
    }

    public GameObject SpawnTown(int x, int y, int p)
    {
        if (unitGrid[x, y] == null)
        {
            // int[] screencoords = MapToScreenCoordinates(x, y);

            Vector3 screencoords = MapToScreenCoordinates(x, y, -1);

            GameObject newTown = Instantiate(strongholdPrefab, screencoords, Quaternion.identity);

            TownScript townScript = newTown.GetComponent<TownScript>();

            townScript.worldManagerObject = this.transform.gameObject;
            townScript.playerControllerObject = playerObjects[p];
            townScript.uiControllerObject = UIControllerObject;

            townList.Add(newTown);

            unitGrid[x, y] = newTown;
            townScript.mapX = x;
            townScript.mapY = y;
            townScript.playerNumber = p;

            players[p].addTown(newTown);

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

            newUnit.transform.position = MapToScreenCoordinates(x, y, -1);
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
        if (!unitList.Remove(u))
        {
            Debug.Log("Error: failed to delete unit from list");
            return false;
        }

        UnitScript uScript = u.GetComponent<UnitScript>();

        Vector2Int unitCoords = uScript.xy();

        unitGrid[uScript.mapX, uScript.mapY] = null;

        if (uScript.GetPlayer() >= 0)
        {
            players[uScript.GetPlayer()].deleteUnit(u);
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

        Vector2Int townCoords = t.GetComponent<TownScript>().xy();

        print(townCoords[0]);
        print(townCoords[1]);

        print(unitGrid[townCoords[0], townCoords[1]]);

        TownScript unitScript = t.GetComponent<TownScript>();

        unitGrid[unitScript.mapX, unitScript.mapY] = null;

        players[t.GetComponent<TownScript>().GetPlayer()].deleteTown(t);

        Destroy(t);

        return true;
    }

    // Takes a pair of map coordinates and translates them to a Vector3 representing a position in rendering space
    // z is defined seperately for sprite layering
    public Vector3 MapToScreenCoordinates(int x, int y, int z)
    {
        int a = x - mapWidth / 2;
        int b = y - mapHeight / 2;

        return new Vector3(x - mapWidth / 2, -y + mapHeight / 2, z);
    }

    // Check if one tile is walkable
    public bool Walkable(int x, int y)
    {
        if (unitGrid[x, y] != null)
        {
            return false;
        }

        if (featureGrid[x, y] == null)
        {
            return (terrainGrid[x, y].GetComponent<TileScript>().walkable);
        }
        else
        {
            if (featureGrid[x, y].tag == "MapFeature")
            {
                return (terrainGrid[x, y].GetComponent<TileScript>().walkable && featureGrid[x, y].GetComponent<MapFeatureScript>().walkable);
            }
            else
            {
                return (terrainGrid[x, y].GetComponent<TileScript>().walkable);
            }
        }
    }

    // Checks if there is an enemy that can be attacked on the tile
    // p is the player who owns the attacking unit
    public bool Attackable(int x, int y, int p)
    {
        if (unitGrid[x, y] == null)
        {
            return false;
        } else
        {
            GameObject o = unitGrid[x, y];
            if(o.CompareTag("Unit"))
            {
                UnitScript u = o.GetComponent<UnitScript>();
                if(u.GetPlayer() == p)
                {
                    // Can't attack your own town
                    return false;
                } else
                {
                    return true;
                }
            } else if(o.CompareTag("Town"))
            {
                TownScript t = o.GetComponent<TownScript>();
                if (t.GetPlayer() == p)
                {
                    // Can't attack your own town
                    return false;
                }
                else
                {
                    return true;
                }
            } else
            {
                return false;
            }
        }

        return false;
    }

    public bool WalkableOrAttackable(int x, int y, int p)
    {
        Debug.Log("checking");
        if (Walkable(x, y)) return true;
        if (Attackable(x, y, p)) return true;
        return false;
    }

    // Takes a pair of map coordinates that mark the corners of a box
    // Returns an array of booleans representing that section of the map, indicating which tiles are walkable
    // Inclusive
    public bool[,] Walkable(Vector2Int a, Vector2Int b)
    {
        if (a.x > b.x || a.y > b.y)
        {
            Debug.Log("Bad coordinates");
            return null;
        }

        bool[,] walkMap = new bool[b.x - a.x + 1, b.y - a.y + 1];

        for (int i = 0; i <= b.x - a.x; i++)
        {
            for (int j = 0; j <= b.y - a.y; j++)
            {
                walkMap[i, j] = Walkable(i + a.x, j + a.y);
            }
        }

        return walkMap;
    }

    // Returns walkable status for the entire map
    public bool[,] WalkableMap()
    {
        return Walkable(new Vector2Int(0, 0), new Vector2Int(mapWidth - 1, mapHeight - 1));
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
        if (e == 0)
        {
            return waterTile;
        }
        else if (e < 5)
        {
            return sandTile;
        }
        else if (e < 20)
        {
            return grassTile;
        }
        else
        {
            return stoneTile;
        }
    }

    public int TownLimit()
    {
        return (int)Math.Floor(Math.Log(turnNumber, 3)) + 1;
    }

    public void CaptureShrine(Vector2Int shrineLocation, int capturingPlayer)
    {
        GameObject shrine = featureGrid[shrineLocation.x, shrineLocation.y];

        if (shrine != null)
        {
            if (shrine.tag == "MapObjective")
            {
                foreach (Player p in players)
                {
                    if (p.playerNumber != capturingPlayer)
                    {
                        p.RemoveShrine(shrine);
                    }
                }

                players[capturingPlayer].AddShrine(shrine);
            }
        }
    }

    public void NextUnit()
    {
        players[currPlayer].NextUnit();
    }

    public void ToggleTechTreeVisible()
    {
        players[currPlayer].ToggleTechTreeVisible();
    }

    public void MoveUnit()
    {
        if (selected != null)
        {
            if (selected.tag == "Unit")
            {
                selected.GetComponent<UnitScript>().Move();
            }
        }
    }

    public void EndTurnButtonPressed()  // this should be moved to the player controller
    {
        endTurnPressed = true;
    }

    public void StartTurn()
    {
        uiController.SetCurrPlayer(players[currPlayer]);

        if (currPlayer == 0)
        {
            turnNumber += 1;
        }

        players[currPlayer].StartTurn();
    }

    public void EndTurn()
    {
        endTurnPressed = false;

        Deselect();

        players[currPlayer].EndOfTurnCleanup();
        currPlayer = (currPlayer + 1) % numPlayers;

        if (currPlayer == 0)
        {
            MoveNeutralUnits();
        }

        StartTurn();
    }

    public Vector2Int GetMapDimensions()
    {
        return new Vector2Int(mapHeight, mapWidth);
        // return new int[] { mapHeight, mapWidth };
    }

    public Player CheckForWinner()
    {
        Player lastActive = null;

        foreach (Player p in players)
        {
            if (p.playerActive)
            {
                if (lastActive == null)
                {
                    lastActive = p;
                }
                else
                {
                    return null;
                }
            }


        }

        if (lastActive != null)
        {
            gameOver = true;
            uiController.SetWinner(lastActive.playerNumber);
        }

        return null;
    }

    public int[] getMapDimensions()
    {
        return new int[] { mapWidth, mapHeight };
    }

    /* public int[] mapToScreenCoordinates(int x, int y)
    {
        int[] mapDimensions = GetMapDimensions();

        int a = x - mapDimensions[1] / 2;
        int b = -y + mapDimensions[0] / 2;

        return new int[] { a, b };
    } */

    void MoveNeutralUnits()
    {
        foreach (GameObject u in unitList)
        {
            NeutralUnitScript s = u.GetComponent<NeutralUnitScript>();
            if (s != null)
            {
                // TODO: Find a bug and re-enable this line
                // s.AutoMove();
                s.ResetMovePoints();
            }
        }
    }

    public int PlayerCount()
    {
        return players.Count();
    }

    // this was a thing for use debugging something but it ended up being unnecessary. huh.
    public int CountUnitsOnGrid()
    {
        int c = 0;
        for (int i = 0; i < unitGrid.GetLength(0); i++)
        {
            for (int j = 0; j < unitGrid.GetLength(1); j++)
            {
                if(unitGrid[i, j] != null)
                {
                    c++;
                }
            }
        }
        return c;
    }

    public static int DistanceBetweenTiles(Vector2Int a, Vector2Int b)
    {
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
    }

    // Testing func
    /*public void AutoExplore()
    {
        ((HumanPlayerController)players[currPlayer].controller).AutoExplore();
    }*/
}
