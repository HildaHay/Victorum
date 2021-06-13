using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HumanPlayerController : PlayerController
{
    public GameObject selectedObject;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (worldManager.currPlayer == player.playerNumber)
        {
            worldManager.Select(GetSelection());

            /*if(Input.GetKeyDown(KeyCode.Q))
            {
                worldManager.AutoExplore();
            }*/
            if (Input.GetKeyDown(KeyCode.E))
            {
                worldManager.EndTurnButtonPressed();
            }
        }
    }

    public GameObject GetSelection()
    {
        if (Input.GetMouseButton(1) && worldManager.gameOver == false)
        {
            selectedObject = null;
            uiManager.SetSelectedObject(null);
        }

        // note: IsPointerOverGameObject is true when the mouse is over UI elements such as buttons
        // to make sure we don't accidentally select or move a unit when clicking a button!
        if (Input.GetMouseButtonDown(0) && worldManager.gameOver == false && !EventSystem.current.IsPointerOverGameObject())
        {

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), new Vector3(0, 0, 1));

            if (hit)
            {
                string hitTag = hit.transform.gameObject.tag;

                switch (hitTag)
                {
                    case "Unit":
                        UnitScript targetUnit = hit.transform.gameObject.GetComponent<UnitScript>();

                        if (targetUnit.GetPlayer() == player.playerNumber)
                        {
                            SelectUnit(hit.transform.gameObject);
                        }
                        else
                        {
                            if (selectedObject)
                            {
                                if (selectedObject.tag == "Unit")
                                {
                                    // check if we can attack the unit
                                    if (selectedObject.GetComponent<UnitScript>().GetPlayer() == player.playerNumber
                                        && targetUnit.GetPlayer() != player.playerNumber)
                                    {
                                        player.AttackUnit(selectedObject.GetComponent<UnitScript>(), targetUnit);
                                    }
                                }
                            }
                        }
                        break;
                    case "Town":
                        TownScript targetTown = hit.transform.gameObject.GetComponent<TownScript>();

                        if (targetTown.playerNumber == player.playerNumber)
                        {
                            SelectTown(hit.transform.gameObject);
                        }
                        else
                        {
                            if (selectedObject)
                            {
                                if (selectedObject.tag == "Unit")
                                {
                                    if (selectedObject.GetComponent<UnitScript>().GetPlayer() == player.playerNumber)
                                    {
                                        UnitScript attacker = selectedObject.GetComponent<UnitScript>();
                                        player.AttackTown(attacker, targetTown);
                                    }
                                }
                            }
                        }
                        break;
                    case "Terrain":
                        TileScript clickedTile = hit.transform.gameObject.GetComponent<TileScript>();

                        Vector2Int clickedTileCoords = new Vector2Int(clickedTile.mapX, clickedTile.mapY);

                        if (selectedObject == null)
                        {
                            int x = hit.transform.gameObject.GetComponent<TileScript>().mapX;
                            int y = hit.transform.gameObject.GetComponent<TileScript>().mapY;

                            if (worldManager.featureGrid[x, y] != null)
                            {
                                if (worldManager.featureGrid[x, y].tag == "MapFeature")
                                {
                                    SelectFeature(worldManager.featureGrid[x, y]);
                                }
                                else if (worldManager.featureGrid[x, y].tag == "MapObjective")
                                {
                                    SelectObjective(worldManager.featureGrid[x, y]);
                                }
                            }
                        }
                        else if (selectedObject.tag == "Unit")
                        {
                            int x = hit.transform.gameObject.GetComponent<TileScript>().mapX;
                            int y = hit.transform.gameObject.GetComponent<TileScript>().mapY;
                            if (worldManager.Walkable(x, y))
                            {
                                selectedObject.GetComponent<UnitScript>().SelectDestination(clickedTileCoords, false);
                            }
                        }
                        else if (selectedObject.tag == "MapFeature")
                        {
                            int x = hit.transform.gameObject.GetComponent<MapFeatureScript>().mapX;
                            int y = hit.transform.gameObject.GetComponent<MapFeatureScript>().mapY;
                            if (worldManager.Walkable(x, y))
                            {
                                selectedObject.GetComponent<UnitScript>().SelectDestination(clickedTileCoords, false);
                            }
                        }
                        else if (selectedObject.tag == "MapObjective")
                        {
                            int x = hit.transform.gameObject.GetComponent<MapObjectiveScript>().mapX;
                            int y = hit.transform.gameObject.GetComponent<MapObjectiveScript>().mapY;
                            if (worldManager.Walkable(x, y))
                            {
                                selectedObject.GetComponent<UnitScript>().SelectDestination(clickedTileCoords, false);
                            }
                        }
                        else
                        {
                            // selected = null;
                        }
                        break;

                }
            }
        }

        return selectedObject;
    }

    // shouldn't be public
    public GameObject SelectUnit(GameObject u)
    {
        if (u.GetComponent<UnitScript>().GetPlayer() == player.playerNumber)
        {
            selectedObject = u;
            uiManager.SetSelectedObject(u);
            return u;
        }
        else
        {
            return null;
        }
    }

    // shouldn't be public
    public GameObject SelectTown(GameObject t)
    {
        uiManager.SetSelectedObject(t);
        if (t.GetComponent<TownScript>().playerNumber == player.playerNumber)
        {
            selectedObject = t;
            t.GetComponent<TownScript>().OpenMenu();
        }
        return t;
    }

    GameObject SelectFeature(GameObject f)
    {
        uiManager.SetSelectedObject(f);
        return f;
    }

    GameObject SelectObjective(GameObject o)
    {
        uiManager.SetSelectedObject(o);
        return o;
    }

    public override GameObject NextUnit()
    {
        foreach (GameObject u in player.UnitList())
        {
            if (u.GetComponent<UnitScript>().GetMovePoints() > 0)
            {
                Camera.main.transform.position = new Vector3(u.transform.position.x, u.transform.position.y, Camera.main.transform.position.z);
                worldManager.Select(u);
                SelectUnit(u);

                return u;
            }
        }
        return null;
    }

    public override void OnTurnStart()
    {
        base.OnTurnStart();
        ClearSelection();
    }

    public void ClearSelection()
    {
        selectedObject = null;
    }

    public override bool IsHuman()
    {
        return true;
    }


    // VVV TESTING FUNCTIONS  - WILL BE REMOVED VVV
    /*public void AutoExplore()
    {
        List<Vector2Int> explorationTargets = FindExplorationTargets(player.GetMainTown().GetComponent<TownScript>().GetLocation(), 2);

        int i = 0;
        foreach (GameObject unit in player.UnitList())
        {
            // Note: If it becomes possible for units to die while attacking, this enumerator will break
            UnitScript s = unit.GetComponent<UnitScript>();
            if (s.GetMovePoints() > 0)
            {
                // MoveUnitRandomly(s);
                // MoveUnitToExplore(s);
                if (i == 0 && explorationTargets.Count >= 1)
                {
                    MoveUnitToLocation(s, explorationTargets[0]);
                }
                else if (i == 1 && explorationTargets.Count >= 2)
                {
                    MoveUnitToLocation(s, explorationTargets[1]);
                }
                else
                {

                }

                i++;
            }
        }
    }

    void MoveUnitToLocation(UnitScript s, Vector2Int l)
    {
        Debug.Log(s.mapX + ", " + s.mapY);
        Debug.Log(l);
        s.SelectDestinationAndMove(l);
        s.ZeroMovePoints();
    }
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
    }*/
    // ^^^ TESTING FUNCTIONS  - WILL BE REMOVED ^^^
}
