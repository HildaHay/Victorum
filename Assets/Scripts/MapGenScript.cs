using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEngine;

public class MapGenScript : MonoBehaviour
{
    public WorldManager worldManager;

    public GameObject grassTile;
    public GameObject sandTile;
    public GameObject dirtTile;
    public GameObject stoneTile;
    public GameObject waterTile;

    public GameObject treePrefab;

    public GameObject[] objectivePrefabs;

    public GameObject neutralUnitPrefab;

    // List<GameObject> units;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public bool GenerateMap(int w, int h)
    // returns true on successful map generation, false on failure
    {


        Debug.Log("Generating terrain");

        int landsize = 650;

        int wOffset = w / 2;
        int hOffset = h / 2;

        //Syl Note : Why are you using C style code here? Why not Use a 2d vector or list?
        int[][] map = new int[w][];
        for (int i = 0; i < h; i++)
        {
            map[i] = new int[w];
        }

        List<int[]> land = new List<int[]>();

        int[] landStart = { w / 2, h / 2 };

        land.Add(landStart);

        int maxElevation = 0;

        while (land.Count < landsize)
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

            if (map[newLand[0]][newLand[1]] == 0)
            {
                // change from water to land
                land.Add(newLand);
                map[newLand[0]][newLand[1]] = 1;
            }
            else
            {
                // increase elevation by 1
                map[newLand[0]][newLand[1]] += 1;
                if (map[newLand[0]][newLand[1]] > maxElevation)
                {
                    maxElevation = map[newLand[0]][newLand[1]];
                }
            }
        }

        Debug.Log("Terrain completed");

        // terrainGrid = new GameObject[mapWidth, mapHeight];
        // featureGrid = new GameObject[mapWidth, mapHeight];

        // create starting towns

        int playerCount = 2;

        bool townsSuccessfullyPlaced = false;

        int[][] townLocations = new int[playerCount][];

        for (int tries = 0; tries < 100; tries++)
        {
            bool placementValid = true;

            for (int i = 0; i < playerCount; i++)
            {
                int r = UnityEngine.Random.Range(0, land.Count());
                townLocations[i] = new int[] { land[r][0], land[r][1] };


                for (int j = 0; j < i; j++)
                {

                    // int distance = Math.Abs(townLocations[i][0] - townLocations[j][0]) + Math.Abs(townLocations[i][1] - townLocations[j][1]);

                    int distance = GetDistance(new Vector2Int(townLocations[i][0], townLocations[i][1]), new Vector2Int(townLocations[j][0], townLocations[j][1]));

                    if (distance < 25)
                    {
                        placementValid = false;
                    }
                }
            }

            if (placementValid)
            {
                townsSuccessfullyPlaced = true;
                break;
            }
        }



        if (townsSuccessfullyPlaced)
        {
            for(int i = 0; i < playerCount; i++)
            {
                worldManager.CreateStartingTown(townLocations[i][0], townLocations[i][1], i);
            }
        }
        else
        {
            print("Could not find valid places for towns");
            return false;
        }

        // create map tiles, features, and shrines

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                GameObject newTile = Instantiate(getTerrainByElevation(map[i][j]), new Vector3(i - wOffset, -j + hOffset, 0), Quaternion.identity);

                newTile.GetComponent<TileScript>().mapX = i;
                newTile.GetComponent<TileScript>().mapY = j;

                worldManager.terrainGrid[i, j] = newTile;

                if (map[i][j] != 0 && worldManager.unitGrid[i, j] == null)
                {
                    int r = UnityEngine.Random.Range(0, 100);

                    if (r >= 0 && r < 10)
                    {
                        GameObject newFeature = Instantiate(treePrefab, new Vector3(i - wOffset, -j + hOffset, -1), Quaternion.identity);

                        worldManager.featureGrid[i, j] = newFeature;

                        newFeature.GetComponent<MapFeatureScript>().mapX = i;
                        newFeature.GetComponent<MapFeatureScript>().mapY = j;
                    }
                    if (r >= 10 && r < 12)
                    {
                        bool validSpawn = true;
                        for(int k = 0; k < playerCount; k++)
                        {

                            if (GetDistance(new Vector2Int(i, j), new Vector2Int(townLocations[k][0], townLocations[k][1])) < 10)
                            {
                                validSpawn = false;
                            }
                        }
                        if(validSpawn)
                        {
                            worldManager.SpawnUnit(neutralUnitPrefab, i, j);
                        }
                    }
                }

                if (map[i][j] != 0 && worldManager.unitGrid[i, j] == null && worldManager.featureGrid[i, j] == null
                    && UnityEngine.Random.Range(0, 100) <= 2)
                {
                    GameObject newObjective = Instantiate(objectivePrefabs[UnityEngine.Random.Range(0, objectivePrefabs.Length)], new Vector3(i - wOffset, -j + hOffset, -1), Quaternion.identity);

                    worldManager.featureGrid[i, j] = newObjective;

                    newObjective.GetComponent<MapObjectiveScript>().mapX = i;
                    newObjective.GetComponent<MapObjectiveScript>().mapY = j;
                }

            }
        }

        return true;
    }
    
    int GetDistance(Vector2Int a, Vector2Int b)
    {
        return Math.Abs(a[0] - b[0]) + Math.Abs(a[1] - b[1]);
    }

    public GameObject getTerrainByElevation(int e)
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
}
