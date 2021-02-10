using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIPlayerController : PlayerController
{
    public GameObject scoutPrefab;  // temp for dev - remove

    int[,] aiMap;  // 0 = unexplored, 1 = walkable, 2 = unwalkable

    // Start is called before the first frame update
    void Start()
    {
        aiMap = new int[worldManager.terrainGrid.GetLength(0), worldManager.terrainGrid.GetLength(1)];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override bool IsHuman()
    {
        return false;
    }

    public override GameObject NextUnit()
    {
        // This function is supposed to get the first unmoved unit for human players
        // For an AI player, it is useless
        return null;
    }

    public override void OnTurnStart()
    {
        base.OnTurnStart();
        BuildAIMap();
        AITakeTurn();
    }

    public void BuildAIMap()
    {
        for(int i = 0; i < worldManager.terrainGrid.GetLength(0); i++)
        {
            for(int j = 0; j < worldManager.terrainGrid.GetLength(1); j++)
            {
                if(!player.tilesExplored[i, j])
                {
                    aiMap[i, j] = 0;
                } else
                {
                    if(worldManager.Walkable(i, j))
                    {
                        aiMap[i, j] = 1;
                    } else
                    {
                        aiMap[i, j] = 2;
                    }
                }
            }
        }
    }

    // Handles everything the AI does on its turn
    public void AITakeTurn()
    {
        foreach (GameObject town in player.TownList())
        {
            if (player.gold > 10)    // cost of a Scout
            {
                player.TownRecruit(town, scoutPrefab);
            }
        }

        foreach (GameObject unit in player.UnitList())
        {
            // Note: If it becomes possible for units to die while attacking, this enumerator will break
            UnitScript s = unit.GetComponent<UnitScript>();
            if (s.GetMovePoints() > 0)
            {
                // MoveUnitRandomly(s);
                MoveUnitToExplore(s);
            }
        }

        worldManager.EndTurn();
    }

    void MoveUnitRandomly(UnitScript s)
    {
        // Moves the unit in random directions until it runs out of movement or hits an obstacle
        while (s.GetMovePoints() > 0)
        {
            Vector2Int startingPosition = s.xy();

            // 0 = up, 1 = down, 2 = left, 3 = right

            string[] directions = { "up", "down", "left", "right" };

            int r = UnityEngine.Random.Range(0, 4);

            Vector2Int targetSquare = startingPosition;

            switch (directions[r])
            {
                case "up":
                    targetSquare.y -= 1;   // Should check if the Y-values for up/down are correct
                    break;
                case "down":
                    targetSquare.y += 1;
                    break;
                case "left":
                    targetSquare.x -= 1;
                    break;
                case "right":
                    targetSquare.x += 1;
                    break;
                default:
                    s.ZeroMovePoints();
                    break;
            }



            if (worldManager.unitGrid[targetSquare.x, targetSquare.y] != null)
            {

                if (worldManager.unitGrid[targetSquare.x, targetSquare.y].tag == "Unit") {
                    player.AttackUnit(s, worldManager.unitGrid[targetSquare.x, targetSquare.y].GetComponent<UnitScript>());
                }
                else
                if (worldManager.unitGrid[targetSquare.x, targetSquare.y].tag == "Town")
                {
                    player.AttackTown(s, worldManager.unitGrid[targetSquare.x, targetSquare.y].GetComponent<TownScript>());
                }
                else
                {
                    s.ZeroMovePoints();
                }
            }
            else
            {
                // Try to move the unit
                if (s.SelectDestinationAndMove(targetSquare) != 2)
                {
                    // If the unit can't move in that direction, end its movement entirely
                    s.ZeroMovePoints();
                }
            }
        }
    }

    void MoveUnitToExplore(UnitScript s)
    {
        TownScript mainTown = player.GetMainTown().GetComponent<TownScript>();
        Vector2Int mainTownCoords = new Vector2Int(mainTown.mapX, mainTown.mapY);

        Vector2Int targetSquare = FindFurthestUnexploredTile(mainTownCoords);

        Debug.Log(mainTownCoords);
        Debug.Log(targetSquare);

        if (s.SelectDestinationAndMove(targetSquare) != 2)
        {
            s.ZeroMovePoints();
        }
    }


    bool IsNextToUnexploredTile(Vector2Int t)
    {
        if(t.x > 0)
        {
            if (player.tilesExplored[t.x - 1, t.y] == false)
                return true;
        }
        if (t.x < worldManager.GetMapDimensions()[0] - 1)
        {
            if (player.tilesExplored[t.x + 1, t.y] == false)
                return true;
        }
        if (t.y > 0)
        {
            if (player.tilesExplored[t.x, t.y - 1] == false)
                return true;
        }
        if (t.y < worldManager.GetMapDimensions()[1] - 1)
        {
            if (player.tilesExplored[t.x, t.y + 1] == false)
                return true;
        }

        return false;
    }

    // NOTE: This works but is very slow! Try to re-work the algorithm so that it works faster
    // Also, make sure the AI can handle the case where it runs out of tiles to explore
    public Vector2Int FindFurthestUnexploredTile(Vector2Int start)
    {
        List<Vector2Int> q = new List<Vector2Int>();

        Vector2Int mapDims = new Vector2Int(worldManager.GetMapDimensions()[0], worldManager.GetMapDimensions()[1]);

        if (start.x < 0 || start.y < 0 || start.x >= mapDims.x || start.y >= mapDims.y)
        {
            Debug.Log("Pathing: start or destination was outside of map");
            return new Vector2Int(-1, -1);
        }

        int[,] distanceVals = new int[mapDims.x, mapDims.y];

        for (int i = 0; i < mapDims.x; i++)
        {
            for (int j = 0; j < mapDims.y; j++)
            {
                distanceVals[i, j] = int.MaxValue;
                if (player.tilesExplored[i, j])
                {
                    q.Add(new Vector2Int(i, j));
                }
            }
        }

        distanceVals[start.x, start.y] = 0;

        Vector2Int furthestPoint = start;

        while(q.Count > 0)
        {
            // find one with min distance
            int min = Int32.MaxValue;
            int minIndex = -1;

            for (int i = 0; i < q.Count; i++)
            {
                Vector2Int curr = q[i];
                if (distanceVals[curr.x, curr.y] < min)
                {
                    min = distanceVals[curr.x, curr.y];
                    minIndex = i;
                }
            }

            Debug.Log(minIndex);

            // If minIndex is still -1, all remaining tiles' distance values are Int32.MaxValue. That means none of them are accessible
            // from the starting tile, so all accessible tiles have already been explored and we can go on to the next step
            if (minIndex >= 0)
            {
                Vector2Int minVertex = q[minIndex];
                q.RemoveAt(minIndex);

                // set neighbours' distance
                if (minVertex.x > 0)
                {
                    if (player.tilesExplored[minVertex.x - 1, minVertex.y] && worldManager.Walkable(minVertex.x - 1, minVertex.y)
                        && distanceVals[minVertex.x - 1, minVertex.y] > distanceVals[minVertex.x, minVertex.y] + 1)
                    {
                        distanceVals[minVertex.x - 1, minVertex.y] = distanceVals[minVertex.x, minVertex.y] + 1;
                    }
                }
                if (minVertex.x < mapDims.x - 1)
                {
                    if (player.tilesExplored[minVertex.x + 1, minVertex.y] && worldManager.Walkable(minVertex.x + 1, minVertex.y)
                        && distanceVals[minVertex.x + 1, minVertex.y] > distanceVals[minVertex.x, minVertex.y] + 1)
                    {
                        distanceVals[minVertex.x + 1, minVertex.y] = distanceVals[minVertex.x, minVertex.y] + 1;
                    }
                }
                if (minVertex.y > 0)
                {
                    if (player.tilesExplored[minVertex.x, minVertex.y - 1] && worldManager.Walkable(minVertex.x, minVertex.y - 1)
                        && distanceVals[minVertex.x, minVertex.y - 1] > distanceVals[minVertex.x, minVertex.y] + 1)
                    {
                        distanceVals[minVertex.x, minVertex.y - 1] = distanceVals[minVertex.x, minVertex.y] + 1;
                    }
                }
                if (minVertex.y < mapDims.y - 1)
                {
                    if (player.tilesExplored[minVertex.x, minVertex.y + 1] && worldManager.Walkable(minVertex.x, minVertex.y + 1)
                        && distanceVals[minVertex.x, minVertex.y + 1] > distanceVals[minVertex.x, minVertex.y] + 1)
                    {
                        distanceVals[minVertex.x, minVertex.y + 1] = distanceVals[minVertex.x, minVertex.y] + 1;
                    }
                }

                // check if this is the most distant tile that is next to an unexplored tile; if it is, it's our new movement target
                if(IsNextToUnexploredTile(minVertex) && distanceVals[furthestPoint.x, furthestPoint.y] <= distanceVals[minVertex.x, minVertex.y])
                {
                    furthestPoint = minVertex;
                }
            }
            else
            {
                q.Clear();
            }

        }

        return furthestPoint;
    }
}