using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenScript : MonoBehaviour
{
    public GameObject gameController;

    public GameObject grassTile;
    public GameObject sandTile;
    public GameObject dirtTile;
    public GameObject stoneTile;
    public GameObject waterTile;
    public GameObject knight;

    List<GameObject> units;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[,] terrainGrid = new GameObject[9, 9];

        for(int i = -4; i <= 4; i++)
        {
            for(int j = -4; j <= 4; j++)
            {
                GameObject newTile;

                if (i == -4 || i == 4 || j == -4 || j == 4)
                {
                    newTile = Instantiate(waterTile, new Vector3(i, -j, 0), Quaternion.identity);
                }
                else
                {
                    int x = Random.Range(1, 4);
                    if (x == 1)
                    {
                        newTile = Instantiate(grassTile, new Vector3(i, -j, 0), Quaternion.identity);
                    }
                    else if (x == 2)
                    {
                        newTile = Instantiate(sandTile, new Vector3(i, -j, 0), Quaternion.identity);
                    }
                    else if (x == 3)
                    {
                        newTile = Instantiate(dirtTile, new Vector3(i, -j, 0), Quaternion.identity);
                    }
                    else
                    {
                        newTile = Instantiate(stoneTile, new Vector3(i, -j, 0), Quaternion.identity);
                    }
                }
                
                newTile.GetComponent<TileScript>().mapX = i + 3;
                newTile.GetComponent<TileScript>().mapY = j + 3;

                terrainGrid[i + 4, j + 4] = newTile;
            }
        }

        gameController.GetComponent<GameControllerScript>().terrainGrid = terrainGrid;

        units = new List<GameObject>();

        units.Add(Instantiate(knight, new Vector3(0, 0, -1), Quaternion.identity));
        units[0].GetComponent<UnitScript>().mapX = 0 + 3;
        units[0].GetComponent<UnitScript>().mapY = 0 + 3; 

    }

    // Update is called once per frame
    void Update()
    {
        bool turnOver = true;

        foreach(GameObject u in units)
        {
            if(u.GetComponent<UnitScript>().CanMove())
            {
                turnOver = false;
            }
        }

        if(turnOver)
        {
            Debug.Log("All units have moved, starting new turn");
            foreach(GameObject u in units)
            {
                u.GetComponent<UnitScript>().ResetMovePoints();
            }
        }
    }
}
