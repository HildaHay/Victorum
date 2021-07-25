using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public WorldManager worldManager;

    public GameObject grassTile;
    public GameObject sandTile;
    public GameObject dirtTile;
    public GameObject stoneTile;
    public GameObject waterTile;

    public GameObject treePrefab;

    public GameObject goldPrefab;

    public GameObject[] objectivePrefabs;

    public GameObject neutralUnitPrefab;

    public int landArea = 50;  // 650
    public int minDistanceBetweenTowns = 5; // 20

    public bool GenerateMap(int w, int h)
    // returns true on successful map generation, false on failure
    {
        Debug.Log("Generating terrain");

        // int landsize = 650;

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

        while (land.Count < landArea)
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
                    int distance = GetDistance(new Vector2Int(townLocations[i][0], townLocations[i][1]), new Vector2Int(townLocations[j][0], townLocations[j][1]));

                    if (distance < minDistanceBetweenTowns)
                    {
                        // make sure the towns are far enough apart
                        placementValid = false;
                    }
                    
                }

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (map[townLocations[i][0] + x][townLocations[i][1] + y] == 0)
                        {
                            // make sure all tiles around the town are walkable land
                            // this ensures that units can move around the town and aren't trapped
                            placementValid = false;
                        }
                    }
                }
            }

            if (placementValid)
            {
                townsSuccessfullyPlaced = true;
                Debug.Log("Found valid town locations in " + tries +" tries");
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

                // testing
                //newTile.GetComponent<SpriteRenderer>().color = Color.blue;

                newTile.GetComponent<TileScript>().mapX = i;
                newTile.GetComponent<TileScript>().mapY = j;

                worldManager.terrainGrid[i, j] = newTile;

                if (map[i][j] != 0 && worldManager.unitGrid[i, j] == null)
                {
                    int r = UnityEngine.Random.Range(0, 100);

                    // Create trees
                    if (r >= 0 && r < 10 && false)  // temporarily disabled cuz it causes problems w/ AI
                    {
                        bool validSpawn = true;
                        for (int k = 0; k < playerCount; k++)
                        {

                            // don't spawn trees next to a town
                            if (GetDistance(new Vector2Int(i, j), new Vector2Int(townLocations[k][0], townLocations[k][1])) < 3)
                            {
                                validSpawn = false;
                            }
                        }
                        if (validSpawn)
                        {
                            GameObject newFeature = Instantiate(treePrefab, new Vector3(i - wOffset, -j + hOffset, -1), Quaternion.identity);

                            worldManager.featureGrid[i, j] = newFeature;

                            newFeature.GetComponent<MapFeatureScript>().mapX = i;
                            newFeature.GetComponent<MapFeatureScript>().mapY = j;
                        }
                    }
                    // Spawning neutral units disabled
                    //if (r >= 10 && r < 12)
                    //{
                    //    bool validSpawn = true;
                    //    for(int k = 0; k < playerCount; k++)
                    //    {

                    //        if (GetDistance(new Vector2Int(i, j), new Vector2Int(townLocations[k][0], townLocations[k][1])) < 10)
                    //        {
                    //            validSpawn = false;
                    //        }
                    //    }

                    //    if(validSpawn)
                    //    {
                    //        worldManager.SpawnUnit(neutralUnitPrefab, i, j);
                    //    }
                    //}
                }

                if (map[i][j] != 0 && worldManager.unitGrid[i, j] == null && worldManager.featureGrid[i, j] == null
                    && UnityEngine.Random.Range(0, 100) <= 2)
                {
                    // spawn shrines

                    bool validSpawn = true;
                    for (int k = 0; k < playerCount; k++)
                    {
                        // Don't spawn shrines within 10 units of players' towns
                        if (GetDistance(new Vector2Int(i, j), new Vector2Int(townLocations[k][0], townLocations[k][1])) < 10)
                        {
                            validSpawn = false;
                        }
                    }

                    if (validSpawn)
                    {
                        GameObject newObjective = Instantiate(objectivePrefabs[UnityEngine.Random.Range(0, objectivePrefabs.Length)], new Vector3(i - wOffset, -j + hOffset, -1), Quaternion.identity);

                        worldManager.featureGrid[i, j] = newObjective;

                        newObjective.GetComponent<MapObjectiveScript>().mapX = i;
                        newObjective.GetComponent<MapObjectiveScript>().mapY = j;
                    }
                }

            }
        }

        // Note: should add some code to make sure each starting town has the same number of resources in its local range, for fairness reasons

        // First-pass resource adding
        // Divides the map into 5x5 sections, and places 2 resource tiles in each
        // Maybe this should happen before other features are added; currently, if either of the selected tiles are already occupied w/
        // other features, the resource is simply left out
        for (int x = 0; x < w; x += 5)
        {
            for (int y = 0; y < h; y += 5)
            {
                // TODO: Make this code more extensible so that the number of resources per section can be modified easily
                Vector2Int resourceLoc1 = new Vector2Int(x + UnityEngine.Random.Range(0, 5), y + UnityEngine.Random.Range(0, 5));
                Vector2Int resourceLoc2 = new Vector2Int(x + UnityEngine.Random.Range(0, 5), y + UnityEngine.Random.Range(0, 5));
                while (resourceLoc1 == resourceLoc2)
                {
                    // Make sure both the resources aren't placed in the same spot
                    resourceLoc2 = new Vector2Int(UnityEngine.Random.Range(0, 5), UnityEngine.Random.Range(0, 5));
                }


                if (worldManager.featureGrid[resourceLoc1.x, resourceLoc1.y] == null && worldManager.terrainGrid[resourceLoc1.x, resourceLoc1.y].GetComponent<TileScript>().walkable)
                {
                    GameObject newResource1 = Instantiate(goldPrefab, new Vector3(resourceLoc1.x - wOffset, -resourceLoc1.y + hOffset, -1), Quaternion.identity);
                    worldManager.featureGrid[resourceLoc1.x, resourceLoc1.y] = newResource1;
                    newResource1.GetComponent<MapFeatureScript>().mapX = x;
                    newResource1.GetComponent<MapFeatureScript>().mapY = y;
                }

                if (worldManager.featureGrid[resourceLoc2.x, resourceLoc2.y] == null && worldManager.terrainGrid[resourceLoc2.x, resourceLoc2.y].GetComponent<TileScript>().walkable)
                {
                    GameObject newResource2 = Instantiate(goldPrefab, new Vector3(resourceLoc2.x - wOffset, -resourceLoc2.y + hOffset, -1), Quaternion.identity);
                    worldManager.featureGrid[resourceLoc2.x, resourceLoc2.y] = newResource2;
                    newResource2.GetComponent<MapFeatureScript>().mapX = x;
                    newResource2.GetComponent<MapFeatureScript>().mapY = y;
                }
            }
        }

        // Second-pass resource adding
        // Goes over every tile, and has a 2% chance of adding a resource to each tile
        // This should probably be placed before the first-pass, for probability reasons
        for(int x = 0; x < w; x++)
        {
            for(int y = 0; y < h; y++)
            {
                if(UnityEngine.Random.Range(0, 100) < 2)
                {
                    if(worldManager.featureGrid[x, y] == null && worldManager.terrainGrid[x, y].GetComponent<TileScript>().walkable)
                    {
                        GameObject newResource = Instantiate(goldPrefab, new Vector3(x - wOffset, -y + hOffset, -1), Quaternion.identity);
                        worldManager.featureGrid[x, y] = newResource;
                        newResource.GetComponent<MapFeatureScript>().mapX = x;
                        newResource.GetComponent<MapFeatureScript>().mapY = y;
                    }
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
