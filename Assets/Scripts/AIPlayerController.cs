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

        worldManager.EndTurn();
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
        RecruitUnits();

        List<Vector2Int> explorationTargets = FindExplorationTargets(player.GetMainTown().GetComponent<TownScript>().GetLocation(), 2);

        // AI behavior: The first unit will be sent to explore at the tile furthest away from the main town; the second unit will be sent
        // to explore at the tile nearest the main town; any other units will move around randomly

        int i = 0;
        foreach (GameObject unit in player.UnitList())
        {
            // Note: If it becomes possible for units to die while attacking, this enumerator will break
            UnitScript s = unit.GetComponent<UnitScript>();
            if (s.GetMovePoints() > 0)
            {
                // MoveUnitRandomly(s);
                // MoveUnitToExplore(s);
                if(i == 0 && explorationTargets.Count >= 1)
                {
                    MoveUnitToLocation(s, explorationTargets[0]);
                } else if(i == 1 && explorationTargets.Count >= 2)
                {
                    MoveUnitToLocation(s, explorationTargets[1]);
                } else
                {
                    MoveUnitRandomly(s);
                }

                i++;
            }
        }
    }

    int ScoutCount()
    {
        int c = 0;
        foreach(GameObject u in player.UnitList())
        {
            if(u.GetComponent<UnitScript>().unitName == "Scout")
            {
                c++;
            }
        }
        return c;
    }

    void RecruitUnits()
    {
        bool[] recruited = new bool[player.TownList().Count];   // Track which towns have already recruited a unit this turn

        int i = 0;
        foreach (GameObject town in player.TownList())
        {
            Debug.Log(recruited[i]);    
            // The AI tries to have two scouts alive at all times; if there are fewer than two, it recruits another
            if (player.Gold() > 10 && ScoutCount() < 2)
            {
                player.TownRecruit(town, scoutPrefab);
                recruited[i] = true;
            }
            i++;
        }


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

    //void MoveUnitToExplore(UnitScript s)
    //{
    //    TownScript mainTown = player.GetMainTown().GetComponent<TownScript>();
    //    Vector2Int mainTownCoords = new Vector2Int(mainTown.mapX, mainTown.mapY);

    //    Vector2Int targetSquare = FindFurthestUnexploredTile(mainTownCoords);

    //    if (s.SelectDestinationAndMove(targetSquare) != 2)
    //    {
    //        s.ZeroMovePoints();
    //    }
    //}

    void MoveUnitToLocation(UnitScript s, Vector2Int l)
    {
        //Debug.Log(s.mapX + ", " + s.mapY);
        //Debug.Log(l);
        s.SelectDestinationAndMove(l);
        s.ZeroMovePoints();
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

    // This makes a list of currently-explored tiles that are adjacent to an unexplored tile, then narrows that list down to a list of
    // "good" targets to prioritize for exploration
    // The list will contain up to two tiles, specificed with argument targetsToFind. If 1, it will return the tile furthest from the
    // player's main town. If 2, it will also return the tile nearest the player's main town.
    // Bug: This can become stuck if the target tile is one that's not accessible (e.g. peninsula blocked by an obstacle)
    public List<Vector2Int> FindExplorationTargets(Vector2Int start, int targetsToFind)
    {
        if(targetsToFind == 0)
        {
            return null;
        }

        Vector2Int mapDimensions = worldManager.GetMapDimensions();

        // Generate a list of all walkable explored tiles that are next to an unexplored tile
        List<Vector2Int> allPossibleTargets = new List<Vector2Int>();

        for (int i = 0; i < mapDimensions.x; i++)
        {
            for(int j = 0; j < mapDimensions.y; j++)
            {
                if(player.tilesExplored[i, j] && worldManager.Walkable(i, j) && IsNextToUnexploredTile(new Vector2Int(i, j))) {
                    allPossibleTargets.Add(new Vector2Int(i, j));
                }
            }
        }

        if(allPossibleTargets.Count == 0)
        {
            // nowhere else to explore
            return null;
        }

        Vector2Int furthest = allPossibleTargets[0];
        int furthestDistance = WorldManager.DistanceBetweenTiles(furthest, start);

        Vector2Int nearest = allPossibleTargets[0];
        int nearestDistance = WorldManager.DistanceBetweenTiles(nearest, start);

        foreach (Vector2Int t in allPossibleTargets)
        {
            if(WorldManager.DistanceBetweenTiles(t, start) > furthestDistance)
            {
                furthest = t;
            }
            if (WorldManager.DistanceBetweenTiles(t, start) < nearestDistance)
            {
                nearest = t;
            }
        }

        List<Vector2Int> targets = new List<Vector2Int>();
        targets.Add(furthest);
        if(targetsToFind >= 2)
        {
            targets.Add(nearest);
        }

        return targets;
    }
}