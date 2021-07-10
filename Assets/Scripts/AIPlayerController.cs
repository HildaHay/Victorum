using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class AIPlayerController : PlayerController
{
    public GameObject scoutPrefab;  // temp for dev - remove
    public GameObject soldierPrefab;    // temp for dev - remove

    int[,] aiMap;  // 0 = unexplored, 1 = walkable, 2 = unwalkable

    public List<TownScript> enemyTowns;

    // Start is called before the first frame update
    void Start()
    {
        aiMap = new int[worldManager.terrainGrid.GetLength(0), worldManager.terrainGrid.GetLength(1)];

        //enemyTowns = new List<TownScript>();
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
        FindEnemyTowns();

        AITakeTurn();

        worldManager.EndTurn();
    }

    public void BuildAIMap()
    {
        for (int i = 0; i < worldManager.terrainGrid.GetLength(0); i++)
        {
            for (int j = 0; j < worldManager.terrainGrid.GetLength(1); j++)
            {
                if (!player.tilesExplored[i, j])
                {
                    aiMap[i, j] = 0;
                }
                else
                {
                    if (worldManager.Walkable(i, j))
                    {
                        aiMap[i, j] = 1;
                    }
                    else
                    {
                        aiMap[i, j] = 2;
                    }
                }
            }
        }
    }

    // Generates a list of all visible enemy towns on the map
    public void FindEnemyTowns()
    {
        enemyTowns = new List<TownScript>();

        for (int i = 0; i < worldManager.terrainGrid.GetLength(0); i++)
        {
            for (int j = 0; j < worldManager.terrainGrid.GetLength(1); j++)
            {
                if(player.tilesExplored[i, j])
                {
                    GameObject u = worldManager.unitGrid[i, j];

                    if (u != null)
                    {
                        if(u.CompareTag("Town"))
                        {
                            TownScript t = u.GetComponent<TownScript>();

                            if(t.playerNumber != player.playerNumber)
                            {
                                enemyTowns.Add(t);
                            }
                        }
                    }
                }
            }
        }
    }

    // Handles everything the AI does on its turn
    public void AITakeTurn()
    {
        RecruitUnits();

        // scouting
        // Note: this bypasses the "Unit Behavior System"
        List<Vector2Int> explorationTargets = FindExplorationTargets(player.GetMainTown().GetComponent<TownScript>().GetLocation(), 2);

        if (explorationTargets != null)
        {
            GameObject[] scouts = GetScouts();

            int i = 0;
            foreach (GameObject s in scouts)
            {
                if (explorationTargets[i] != null)
                {
                    MoveUnitToLocation(s.GetComponent<UnitScript>(), explorationTargets[i]);
                }
            }
        }

        foreach (GameObject unit in player.UnitList())
        {
            UnitScript s = unit.GetComponent<UnitScript>();
            if (enemyTowns.Count > 0)
            {
                // check if next to a town it can attack
                GameObject adjacentTown = GetAdjacentEnemyTown(s);
                if (adjacentTown != null) {
                    s.AttackTarget(adjacentTown);
                }
                else
                {
                    Vector2Int target = ApproachNearestEnemyTown.Execute(this, s);
                    MoveUnitToLocation(s, target);
                }
            } else
            {
                MoveUnitRandomly(s);
            }
        }

        // AI behavior: The first unit will be sent to explore at the tile furthest away from the main town; the second unit will be sent
        // to explore at the tile nearest the main town; any other units will move around randomly

        //int i = 0;
        //foreach (GameObject unit in player.UnitList())
        //{
        //    // Note: If it becomes possible for units to die while attacking, this enumerator will break
        //    UnitScript s = unit.GetComponent<UnitScript>();
        //    if (s.GetMovePoints() > 0)
        //    {
        //        // MoveUnitRandomly(s);
        //        // MoveUnitToExplore(s);
        //        if(i == 0 && explorationTargets.Count >= 1)
        //        {
        //            MoveUnitToLocation(s, explorationTargets[0]);
        //        } else if(i == 1 && explorationTargets.Count >= 2)
        //        {
        //            MoveUnitToLocation(s, explorationTargets[1]);
        //        } else
        //        {
        //            MoveUnitRandomly(s);
        //        }

        //        i++;
        //    }
        //}
    }

    // Temporary function; checks if a unit's next to an enemy town, to attack that town.
    GameObject GetAdjacentEnemyTown(UnitScript u)
    {
        Vector2Int location = u.xy();

        // i know this code sucks, it's just temporary though

        // Above
        if (worldManager.unitGrid[location.x - 1, location.y] != null)
        {
            if(worldManager.unitGrid[location.x - 1, location.y].CompareTag("Town"))
            {
                if(worldManager.unitGrid[location.x - 1, location.y].GetComponent<TownScript>().playerNumber != player.playerNumber) {
                    return worldManager.unitGrid[location.x - 1, location.y];
                }
            }
        }
        // Below
        if (worldManager.unitGrid[location.x + 1, location.y] != null)
        {
            if (worldManager.unitGrid[location.x + 1, location.y].CompareTag("Town"))
            {
                if (worldManager.unitGrid[location.x + 1, location.y].GetComponent<TownScript>().playerNumber != player.playerNumber)
                {
                    return worldManager.unitGrid[location.x + 1, location.y];
                }
            }
        }
        // Right
        if (worldManager.unitGrid[location.x, location.y + 1] != null)
        {
            if (worldManager.unitGrid[location.x, location.y + 1].CompareTag("Town"))
            {
                if (worldManager.unitGrid[location.x, location.y + 1].GetComponent<TownScript>().playerNumber != player.playerNumber)
                {
                    return worldManager.unitGrid[location.x, location.y + 1];
                }
            }
        }
        // Left
        if (worldManager.unitGrid[location.x, location.y - 1] != null)
        {
            if (worldManager.unitGrid[location.x, location.y - 1].CompareTag("Town"))
            {
                if (worldManager.unitGrid[location.x, location.y - 1].GetComponent<TownScript>().playerNumber != player.playerNumber)
                {
                    return worldManager.unitGrid[location.x, location.y - 1];
                }
            }
        }

        return null;
    }

    // This function calculates priority values for different strategies, which will influence the behavior of individual units
    int[] TopLevelStrategy()
    {
        // Strategies are: Explore, Claim, Attack, Defend
        int[] s = new int[] { 10, 10, 10, 10 };

        return s;
    }

    // for future implementation
    void BehaviorSelection(UnitScript u)
    {

    }


    // for future implementation
    void IndividualAction(UnitScript u, int behavior)   // "behavior" should be an enum or possibly object
    {

    }

    int ScoutCount()
    {
        int c = 0;
        foreach (GameObject u in player.UnitList())
        {
            if (u.GetComponent<UnitScript>().unitName == "Scout")
            {
                c++;
            }
        }
        return c;
    }

    GameObject[] GetScouts()
    {
        GameObject[] scoutUnits = new GameObject[] { null, null };
        int s = 0;

        foreach (GameObject u in player.UnitList())
        {
            if (u.GetComponent<UnitScript>().unitName == "Scout")
            {
                scoutUnits[s] = u;
                s++;
                if (s == 2)
                {
                    return scoutUnits;
                }
            }
        }
        if (s == 1)
        {
            return scoutUnits.Take(1).ToArray();
        }
        else
        {
            return null;
        }
    }

    void RecruitUnits()
    {
        bool[] recruited = new bool[player.TownList().Count];   // Track which towns have already recruited a unit this turn

        int i = 0;
        foreach (GameObject town in player.TownList())
        {
            // The AI tries to have two scouts alive at all times; if there are fewer than two, it recruits another
            if (player.Gold() > 10 && ScoutCount() < 2)
            {
                player.TownRecruit(town, scoutPrefab);
                recruited[i] = true;
            } else if(player.Gold() > 10)
            {
                player.TownRecruit(town, soldierPrefab);
            }
            i++;
        }


    }

    void MoveUnitRandomly(UnitScript s)
    {
        int escape = 0; // Stopgap to break infinite loops

        // Moves the unit in random directions until it runs out of movement or hits an obstacle
        while (s.GetMovePoints() > 0)
        {
            Vector2Int targetSquare = RandomMoveOnce.Execute(this, s);

            if (worldManager.unitGrid[targetSquare.x, targetSquare.y] != null)
            {

                if (worldManager.unitGrid[targetSquare.x, targetSquare.y].tag == "Unit")
                {
                    player.AttackUnit(s, worldManager.unitGrid[targetSquare.x, targetSquare.y].GetComponent<UnitScript>());
                    s.ZeroMovePoints();
                    break;
                }
                else
                if (worldManager.unitGrid[targetSquare.x, targetSquare.y].tag == "Town")
                {
                    player.AttackTown(s, worldManager.unitGrid[targetSquare.x, targetSquare.y].GetComponent<TownScript>());
                    s.ZeroMovePoints();
                    break;
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

            escape++;
            if(escape >= 20)
            {
                s.ZeroMovePoints();
                break;
            }
        }
    }

    void MoveUnitToLocation(UnitScript s, Vector2Int l)
    {
        s.SelectDestinationAndMove(l);
        s.ZeroMovePoints();
    }

    // Checks if a tile is revealed, and if so, if it has any adjacent tiles that aren't revealed yet
    bool IsNextToUnexploredTile(Vector2Int t)
    {
        if (t.x > 0)
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
        if (targetsToFind == 0)
        {
            return null;
        }

        Vector2Int mapDimensions = worldManager.GetMapDimensions();

        // Generate a list of all walkable explored tiles that are next to an unexplored tile
        List<Vector2Int> allPossibleTargets = new List<Vector2Int>();

        for (int i = 0; i < mapDimensions.x; i++)
        {
            for (int j = 0; j < mapDimensions.y; j++)
            {
                if (player.tilesExplored[i, j] && worldManager.Walkable(i, j) && IsNextToUnexploredTile(new Vector2Int(i, j)))
                {
                    allPossibleTargets.Add(new Vector2Int(i, j));
                }
            }
        }

        if (allPossibleTargets.Count == 0)
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
            if (WorldManager.DistanceBetweenTiles(t, start) > furthestDistance)
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
        if (targetsToFind >= 2)
        {
            targets.Add(nearest);
        }

        return targets;
    }
    
}

// Behavior Functions
// These functions take the AIPlayerController and a UnitScript as arguments, and return coordinates for the unit to move to
// They are set up modularly, so units can pick which behavior they would like to use, and the list of possible behaviors can be expanded
// and individual behaviors can be tweaked easily without having to change the units' code

class UnitBehavior
{
    public static Vector2Int Execute(AIPlayerController p, UnitScript u)
    {
        return new Vector2Int(0, 0);
    }
}

class RandomMoveOnce : UnitBehavior
{
    public static Vector2Int Execute(AIPlayerController p, UnitScript u)
    {
        Vector2Int startingPosition = u.xy();

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
            default:    // move down
                targetSquare.y += 1;
                break;
        }

        return targetSquare;
    }
}

class ApproachNearestEnemyTown : UnitBehavior
{
    // This function moves the unit to a tile adjacent to the enemy's town
    // It will be replaced with a function AttackNearestEnemyTown, whicn instead will return the town's location. However, the way that
    // unit movement is implemented makes this not work yet
    public static Vector2Int Execute(AIPlayerController p, UnitScript u)
    {
        // This code is messy and could use some cleanup
        List<TownScript> targets = p.enemyTowns;
        if (targets.Count == 0)
        {
            return new Vector2Int(0, 0);
        }

        Vector2Int location = u.xy();

        TownScript nearestTown = targets[0];
        int nearestDistance = WorldManager.DistanceBetweenTiles(location, nearestTown.GetLocation());

        foreach(TownScript t in targets)
        {
            Vector2Int townLocation = t.GetLocation();
            int d = WorldManager.DistanceBetweenTiles(location, t.GetLocation());
            if(d < nearestDistance)
            {
                nearestTown = t;
                nearestDistance = d;
            }
        }

        if (nearestTown != null)
        {
            Vector2Int townLocation = nearestTown.GetLocation();
            // Move to one of the four tiles adjacent to the town
            // Above
            if (p.worldManager.Walkable(townLocation.x - 1, townLocation.y))
            {
                return townLocation + new Vector2Int(-1, 0);
            }
            // Below
            if (p.worldManager.Walkable(townLocation.x + 1, townLocation.y))
            {
                return townLocation + new Vector2Int(1, 0);
            }
            // Left
            if (p.worldManager.Walkable(townLocation.x, townLocation.y - 1))
            {
                return townLocation + new Vector2Int(0, -1);
            }
            // Right
            if (p.worldManager.Walkable(townLocation.x, townLocation.y + 1))
            {
                return townLocation + new Vector2Int(0, 1);
            }

            // all tiles are occupied
            return new Vector2Int(0, 0);
        }
        else
        {
            return new Vector2Int(0, 0);
        }
    }
}