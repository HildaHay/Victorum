using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerScript : MonoBehaviour
{

    GameObject selected;

    public GameObject[,] terrainGrid;
    public GameObject[,] unitGrid;

    public GameObject grassTile;
    public GameObject sandTile;
    public GameObject dirtTile;
    public GameObject stoneTile;
    public GameObject waterTile;

    public List<GameObject> unitList;

    public GameObject knightPrefab;
    public GameObject strongholdPrefab;

    int mapWidth;
    int mapHeight;

    // Start is called before the first frame update
    void Start()
    {
        mapWidth = 17;
        mapHeight = 11;

        terrainGrid = new GameObject[mapWidth, mapHeight];
        unitGrid = new GameObject[mapWidth, mapHeight];

        GenerateMap(mapWidth, mapHeight);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), new Vector3(0, 0, 1));

            if (hit)
            {
                if (hit.transform.gameObject.tag == "Unit")
                {
                    selected = hit.transform.gameObject;
                    print(selected);
                }
                else if (hit.transform.gameObject.tag == "Town")
                {
                    int x = hit.transform.gameObject.GetComponent<TownScript>().mapX;
                    int y = hit.transform.gameObject.GetComponent<TownScript>().mapY - 1;

                    if (terrainGrid[x, y] != null)
                    {
                        GameObject newUnit = hit.transform.gameObject.GetComponent<TownScript>().CreateUnit();
                        if (newUnit != null)
                        {
                            unitList.Add(newUnit);
                            terrainGrid[x, y] = newUnit;
                            newUnit.GetComponent<KnightScript>().mapX = x;
                            newUnit.GetComponent<KnightScript>().mapY = y;

                            int[] screenCoords = mapToScreenCoordinates(x, y);
                            newUnit.transform.position = new Vector3(screenCoords[0], screenCoords[1], -1);
                        }
                    }
                    else
                    {
                        Debug.Log("Space occupied");
                    }

                    // spawnUnit(x, y - 1);

                    print("Spawning unit");

                } else if(hit.transform.gameObject.tag == "Terrain")
                {
                    if (selected)
                    {
                        if (hit.transform.gameObject.GetComponent<TileScript>().walkable)
                        {
                            moveUnit(selected, hit.transform.gameObject.GetComponent<TileScript>().mapX, hit.transform.gameObject.GetComponent<TileScript>().mapY);
                        }
                    }
                }
                
                // Do something with the object that was hit by the raycast.
            }
        }

        bool turnOver = true;

        foreach (GameObject u in unitList)
        {
            if (u.tag == "Unit")
            {
                if (u.GetComponent<KnightScript>().CanMove())
                {
                    turnOver = false;
                }
            } else if (u.tag == "Town")
            {
                if (u.GetComponent<TownScript>().CanBuy())
                {
                    turnOver = false;
                }
            }
        }

        if (turnOver)
        {
            Debug.Log("All units have moved, starting new turn");
            foreach (GameObject u in unitList)
            {
                if (u.tag == "Unit")
                {
                    u.GetComponent<KnightScript>().ResetMovePoints();
                }
                if (u.tag == "Town")
                {
                    u.GetComponent<TownScript>().TurnStart();
                }
            }
        }
    }

    public GameObject spawnStronghold(int x, int y)
    {
        if (terrainGrid[x, y] != null)
        {
            int[] screencoords = mapToScreenCoordinates(x, y);

            GameObject newStronghold = Instantiate(strongholdPrefab, new Vector3(screencoords[0], screencoords[1], -1), Quaternion.identity);
            unitList.Add(newStronghold);
            terrainGrid[x, y] = newStronghold;
            newStronghold.GetComponent<TownScript>().mapX = x;
            newStronghold.GetComponent<TownScript>().mapY = y;
            return null;
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

        spawnStronghold(wOffset, hOffset);
    }

    public int[] mapToScreenCoordinates(int x, int y)
    {
        int a = x - mapWidth / 2;
        int b = y - mapHeight / 2;

        print(a + ", " + b);

        return new int[] { x - mapWidth / 2, -y + mapHeight / 2};
    }

    bool moveUnit(GameObject unit, int x, int y)
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

                int[] newScreenCords = mapToScreenCoordinates(x, y);
                unit.transform.position = new Vector3(newScreenCords[0], newScreenCords[1], -1);

                return true;
            }
            else
            {
                Debug.Log("Out of movement");
                return false;
            }
        }
    }
}
