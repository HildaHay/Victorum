using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerScript : MonoBehaviour
{

    GameObject selected;

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
    
    public int currPlayer;
    public int numPlayers = 2;

    bool endTurnPressed;

    // Start is called before the first frame update
    void Start()
    {
        mapWidth = 17;
        mapHeight = 11;

        terrainGrid = new GameObject[mapWidth, mapHeight];
        unitGrid = new GameObject[mapWidth, mapHeight];

        uiController = UIControllerObject.GetComponent<UIControllerScript>();

        playerControllerObjects = new GameObject[2];
        playerControllers = new PlayerControllerScript[2];

        for (int i = 0; i < numPlayers; i++)
        {
            playerControllerObjects[i] = Instantiate(playerControllerPrefab);
            playerControllers[i] = playerControllerObjects[i].GetComponent<PlayerControllerScript>();
            playerControllers[i].gameControllerScript = this;
            playerControllers[i].uiScript = uiController;
        }

        GenerateMap(mapWidth, mapHeight);

        currPlayer = 0;
        uiController.SetCurrPlayer(currPlayer);

        endTurnPressed = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("up"))
        {

        }

        selected = playerControllers[0].getPlayerSelection(selected);

        if(endTurnPressed)
        {
            print("e");
            EndTurn();
        }
    }

    public GameObject SpawnStronghold(int x, int y, int p)
    {
        if (terrainGrid[x, y] != null)
        {
            int[] screencoords = MapToScreenCoordinates(x, y);

            GameObject newStronghold = Instantiate(strongholdPrefab, new Vector3(screencoords[0], screencoords[1], -1), Quaternion.identity);
            unitList.Add(newStronghold);
            terrainGrid[x, y] = newStronghold;
            newStronghold.GetComponent<TownScript>().mapX = x;
            newStronghold.GetComponent<TownScript>().mapY = y;
            newStronghold.GetComponent<TownScript>().player = p;
            return newStronghold;
        }
        else
        {
            Debug.Log("Space occupied");
            return null;
        }
    }

    void GenerateMap(int w, int h)
    {
        int wOffset = w / 2;
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
        GameObject enemyTown = SpawnStronghold(wOffset + 3, hOffset, 1);
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

                uiController.ShowUnitInfo(unit);

                return true;
            }
            else
            {
                uiController.ShowUnitInfo(unit);
                Debug.Log("Out of movement");
                
                return false;
            }
        }
    }

    public void EndTurnButtonPressed()
    {
        endTurnPressed = true;
    }

    public void EndTurn()
    {
        playerControllers[0].EndTurn();
        endTurnPressed = false;
    }

    public int[] GetMapDimensions()
    {
        return new int[] {mapHeight, mapWidth };
    }
}
