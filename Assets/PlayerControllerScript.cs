using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerControllerScript : MonoBehaviour
{
    // GameObject selected;

    GameControllerScript gameController;
    UIControllerScript uiController;

    List<GameObject> playerUnitList;
    List<GameObject> playerTownList;
    List<GameObject> playerShrineList;

    bool[,] tilesExplored;

    GameObject mainTown;

    public int playerNumber;

    Camera mainCamera;

    Vector3 playerCameraPosition;

    public bool playerActive;

    int unitVisionDistance = 3;
    int townVisionDistance = 6;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main; //y tho?

        playerCameraPosition = mainTown.transform.position + new Vector3(0, 0, -10);

        playerShrineList = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Initialize(int p, GameControllerScript gC, UIControllerScript uiC)
    {
        playerNumber = p;
        gameController = gC;
        uiController = uiC;

        playerActive = true;

        playerUnitList = new List<GameObject>();
        playerTownList = new List<GameObject>();

        tilesExplored = new bool[gameController.getMapDimensions()[0], gameController.getMapDimensions()[1]];
    }

    public void CheckVision(int x, int y, int visionRange)
    {
        int xmin = Math.Max(0, x - visionRange);
        int xmax = Math.Min(tilesExplored.Length, x + visionRange);
        int ymin = Math.Max(0, y - visionRange);
        int ymax = Math.Min(tilesExplored.Length, y + visionRange);

        for (int i = xmin; i < xmax; i++)
        {
            for (int j = ymin; j < ymax; j++)
            {
                if (Math.Abs(x - i) + Math.Abs(y - j) < visionRange)
                {
                    tilesExplored[i, j] = true;
                    SetTileVisibility(i, j, tilesExplored[i, j]);
                }
            }
        }
    }

    GameObject SelectUnit(GameObject u)
    {

        if(u.GetComponent<UnitScript>().GetPlayer() == playerNumber)
        {
            uiController.SetSelectedObject(u);
            return u;
        } else
        {
            return null;
        }
    }

    GameObject SelectTown(GameObject t)
    {
        uiController.SetSelectedObject(t);
        return t;
    }

    GameObject SelectFeature(GameObject f)
    {
        uiController.SetSelectedObject(f);
        return f;
    }

    GameObject SelectObjective(GameObject o)
    {
        uiController.SetSelectedObject(o);
        return o;
    }

    public GameObject TownRecruit(GameObject t, GameObject unitToBuild)
    {
        TownScript townScript = t.GetComponent<TownScript>();

        int x = townScript.mapX;
        int y = townScript.mapY + 1;
        
        GameObject newUnit = null;

        if (gameController.unitGrid[x, y] == null)
        {
            newUnit = townScript.BuildUnit(unitToBuild);
            if (newUnit != null)
            {
                gameController.unitList.Add(newUnit);
                gameController.unitGrid[x, y] = newUnit;
                newUnit.GetComponent<UnitScript>().mapX = x;
                newUnit.GetComponent<UnitScript>().mapY = y;

                int[] screenCoords = mapToScreenCoordinates(x, y);
                newUnit.transform.position = new Vector3(screenCoords[0], screenCoords[1], -1);
            }
        }
        else
        {
            Debug.Log("Space occupied");
        }

        // uiScript.SetSelectedObject(t);

        return newUnit;
    }

    /* public void Combat(UnitScript attacker, UnitScript defender)
    {
        int aDmg = attacker.attackDamage();
        int dDmg = defender.attackDamage();

        defender.receiveDamage(aDmg);
        attacker.receiveDamage(dDmg);
    } */

    public GameObject getPlayerSelection(GameObject s)
    {
        GameObject selected = s;    // Necessary?

        if(Input.GetMouseButton(1) && gameController.gameOver == false)
        {
            selected = null;
            uiController.SetSelectedObject(null);
        }

        if (Input.GetMouseButtonDown(0) && gameController.gameOver == false)
        {

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), new Vector3(0, 0, 1));

            if (hit)
            {
                string hitTag = hit.transform.gameObject.tag;

                switch(hitTag)
                {
                    case "Unit":
                        if (selected)
                        {
                            // check if we can attack the unit
                            if (selected.tag == "Unit")
                            {
                                UnitScript attacker = selected.GetComponent<UnitScript>();
                                UnitScript target = hit.transform.gameObject.GetComponent<UnitScript>();
                                AttackUnit(attacker, target);
                            }
                            else
                            {
                                // select the unit
                                selected = SelectUnit(hit.transform.gameObject);
                            }
                        }
                        else
                        {
                            selected = SelectUnit(hit.transform.gameObject);
                        }
                        break;
                    case "Town":
                        if (selected)
                        {
                            if (selected.tag == "Unit")
                            {
                                UnitScript attacker = selected.GetComponent<UnitScript>();
                                TownScript target = hit.transform.gameObject.GetComponent<TownScript>();
                                AttackTown(attacker, target);
                            }
                            else
                            {

                                selected = SelectTown(hit.transform.gameObject);
                                if (selected.GetComponent<TownScript>().player == playerNumber)
                                {
                                    selected.GetComponent<TownScript>().OpenMenu();
                                }
                            }
                        }
                        else
                        {

                            selected = SelectTown(hit.transform.gameObject);
                            if (selected.GetComponent<TownScript>().player == playerNumber)
                            {
                                selected.GetComponent<TownScript>().OpenMenu();
                            }
                        }
                        break;
                    case "Terrain":
                        if (selected == null)
                        {
                            int x = hit.transform.gameObject.GetComponent<TileScript>().mapX;
                            int y = hit.transform.gameObject.GetComponent<TileScript>().mapY;

                            if (gameController.featureGrid[x, y] != null)
                            {
                                if (gameController.featureGrid[x, y].tag == "MapFeature")
                                {
                                    SelectFeature(gameController.featureGrid[x, y]);
                                }
                                else if (gameController.featureGrid[x, y].tag == "MapObjective")
                                {
                                    SelectObjective(gameController.featureGrid[x, y]);
                                }
                            }
                        }
                        else if (selected.tag == "Unit")
                        {
                            int x = hit.transform.gameObject.GetComponent<TileScript>().mapX;
                            int y = hit.transform.gameObject.GetComponent<TileScript>().mapY;
                            if (gameController.Walkable(x, y))
                            {
                                gameController.MoveUnit(selected, hit.transform.gameObject.GetComponent<TileScript>().mapX, hit.transform.gameObject.GetComponent<TileScript>().mapY);
                            }
                        }
                        else if (selected.tag == "MapFeature")
                        {
                            int x = hit.transform.gameObject.GetComponent<MapFeatureScript>().mapX;
                            int y = hit.transform.gameObject.GetComponent<MapFeatureScript>().mapY;
                            if (gameController.Walkable(x, y))
                            {
                                gameController.MoveUnit(selected, hit.transform.gameObject.GetComponent<TileScript>().mapX, hit.transform.gameObject.GetComponent<TileScript>().mapY);
                            }
                        }
                        else if (selected.tag == "MapObjective")
                        {
                            int x = hit.transform.gameObject.GetComponent<MapObjectiveScript>().mapX;
                            int y = hit.transform.gameObject.GetComponent<MapObjectiveScript>().mapY;
                            if (gameController.Walkable(x, y))
                            {
                                gameController.MoveUnit(selected, hit.transform.gameObject.GetComponent<TileScript>().mapX, hit.transform.gameObject.GetComponent<TileScript>().mapY);
                            }
                        }
                        else
                        {
                            // selected = null;
                        }
                        break;

                }

                /* if (hit.transform.gameObject.tag == "Unit")
                {
                    if (selected)
                    {
                        // check if we can attack the unit
                        if (selected.tag == "Unit")
                        {
                            UnitScript attacker = selected.GetComponent<UnitScript>();
                            UnitScript target = hit.transform.gameObject.GetComponent<UnitScript>();
                            AttackUnit(attacker, target);
                        }
                        else
                        {
                            // select the unit
                            selected = SelectUnit(hit.transform.gameObject);
                        }
                    }
                    else
                    {
                        selected = SelectUnit(hit.transform.gameObject);
                    }
                }
                else if (hit.transform.gameObject.tag == "Town")
                {
                    if (selected)
                    {
                        if (selected.tag == "Unit")
                        {
                            UnitScript attacker = selected.GetComponent<UnitScript>();
                            TownScript target = hit.transform.gameObject.GetComponent<TownScript>();
                            AttackTown(attacker, target);
                        } else
                        {

                            selected = SelectTown(hit.transform.gameObject);
                            if (selected.GetComponent<TownScript>().player == playerNumber)
                            {
                                selected.GetComponent<TownScript>().OpenMenu();
                            }
                        }
                    }
                    else
                    {

                        selected = SelectTown(hit.transform.gameObject);
                        if (selected.GetComponent<TownScript>().player == playerNumber)
                        {
                            selected.GetComponent<TownScript>().OpenMenu();
                        }
                    }

                }
                else if (hit.transform.gameObject.tag == "Terrain")
                {
                    if(selected == null)
                    {
                        int x = hit.transform.gameObject.GetComponent<TileScript>().mapX;
                        int y = hit.transform.gameObject.GetComponent<TileScript>().mapY;
                        
                        if (gameController.featureGrid[x, y] != null)
                        {
                            if (gameController.featureGrid[x, y].tag == "MapFeature")
                            {
                                SelectFeature(gameController.featureGrid[x, y]);
                            }
                            else if (gameController.featureGrid[x, y].tag == "MapObjective")
                            {
                                SelectObjective(gameController.featureGrid[x, y]);
                            }
                        }
                    }
                    else if (selected.tag == "Unit")
                    {
                        int x = hit.transform.gameObject.GetComponent<TileScript>().mapX;
                        int y = hit.transform.gameObject.GetComponent<TileScript>().mapY;
                        if (gameController.Walkable(x, y))
                        {
                            gameController.MoveUnit(selected, hit.transform.gameObject.GetComponent<TileScript>().mapX, hit.transform.gameObject.GetComponent<TileScript>().mapY);
                        }
                    }
                    else if (selected.tag == "MapFeature")
                    {
                        int x = hit.transform.gameObject.GetComponent<MapFeatureScript>().mapX;
                        int y = hit.transform.gameObject.GetComponent<MapFeatureScript>().mapY;
                        if (gameController.Walkable(x, y))
                        {
                            gameController.MoveUnit(selected, hit.transform.gameObject.GetComponent<TileScript>().mapX, hit.transform.gameObject.GetComponent<TileScript>().mapY);
                        }
                    }
                    else if (selected.tag == "MapObjective")
                    {
                        int x = hit.transform.gameObject.GetComponent<MapObjectiveScript>().mapX;
                        int y = hit.transform.gameObject.GetComponent<MapObjectiveScript>().mapY;
                        if (gameController.Walkable(x, y))
                        {
                            gameController.MoveUnit(selected, hit.transform.gameObject.GetComponent<TileScript>().mapX, hit.transform.gameObject.GetComponent<TileScript>().mapY);
                        }
                    }
                    else
                    {
                        // selected = null;
                    }
                }*/
 
                // Do something with the object that was hit by the raycast.
            }
        }

        return selected;
    }

    public void AttackTown(UnitScript attacker, TownScript target)
    {
        if (attacker.GetComponent<UnitScript>().GetPlayer() == playerNumber
            && target.GetComponent<TownScript>().GetPlayer() != playerNumber)
        {
            if (attacker.TryAttackTown(target))
            {
                target.ReceiveDamage(attacker.AttackDamage());
            }
        } else
        {
            SelectTown(target.gameObject);
        }
    }
    public void AttackUnit(UnitScript attacker, UnitScript target)
    {
        if (attacker.GetComponent<UnitScript>().GetPlayer() == playerNumber
            && target.GetComponent<UnitScript>().GetPlayer() != playerNumber)
        {
            if (attacker.TryAttack(target))
            {
                target.ReceiveDamage(attacker.AttackDamage());
            }
        } else
        {
            SelectUnit(target.gameObject);
        }
    }

    public void StartTurn()
    {

        // mainCamera.transform.position = mainTown.transform.position + new Vector3(0, 0, -10);
        mainCamera.transform.position = playerCameraPosition;

        foreach (GameObject u in playerUnitList)
        {
            u.GetComponent<UnitScript>().ResetMovePoints();
        }
        foreach(GameObject t in playerTownList)
        {
            TownScript s = t.GetComponent<TownScript>();
            s.TurnStart();
            CheckVision(s.mapX, s.mapY, townVisionDistance);
        }

        ShowExplored();
    }

    public void EndTurn()
    {
        HideAll();

        playerCameraPosition = mainCamera.transform.position;

        uiController.SetSelectedObject(null);

        Debug.Log("Player " + playerNumber + " Shrines: " + playerShrineList.Count);
    }

    public int[] mapToScreenCoordinates(int x, int y)
    {
        int[] mapDimensions = gameController.GetMapDimensions();

        int a = x - mapDimensions[1] / 2;
        int b = -y + mapDimensions[0] / 2;

        return new int[] { a, b };
    }

    public GameObject addUnit(GameObject newUnit)
    {
        playerUnitList.Add(newUnit);

        return newUnit;
    }

    public GameObject addTown(GameObject newTown)
    {
        playerTownList.Add(newTown);

        return newTown;
    }

    public GameObject setMainTown(GameObject t)
    {
        mainTown = t;
        return mainTown;
    } 

    public bool deleteUnit(GameObject u)
    {
        playerUnitList.Remove(u);

        return true;
    }

    public bool deleteTown(GameObject t)    // could possibly be combined with deleteUnit
    {

        playerTownList.Remove(t);
        
        if(playerTownList.Count <= 0)
        {
            playerActive = false;
            gameController.CheckForWinner();
        }

        return true;
    }

    void HideAll()
    {
        GameObject[,] terrainGrid = gameController.terrainGrid;

        for(int i = 0; i < terrainGrid.GetLength(0); i++)
        {
            for (int j = 0; j < terrainGrid.GetLength(1); j++)
            {
                SetTileVisibility(i, j, false);
            }
        }
    }
    void ShowAll()
    {
        for (int i = 0; i < gameController.terrainGrid.GetLength(0); i++)
        {
            for (int j = 0; j < gameController.terrainGrid.GetLength(1); j++)
            {
                SetTileVisibility(i, j, true);
            }
        }
    }

    void ShowExplored()
    {
        for (int i = 0; i < gameController.terrainGrid.GetLength(0); i++)
        {
            for (int j = 0; j < gameController.terrainGrid.GetLength(1); j++)
            {
                SetTileVisibility(i, j, tilesExplored[i, j]);
                /* if(tilesExplored[i, j] == true)
                {
                    gameController.terrainGrid[i, j].GetComponent<TileScript>().tileRenderer.enabled = true;
                }
                else
                {
                    gameController.terrainGrid[i, j].GetComponent<TileScript>().tileRenderer.enabled = false;
                } */
            }
        }
    }

    void SetTileVisibility(int x, int y, bool visible)
    {
        gameController.terrainGrid[x, y].GetComponent<TileScript>().tileRenderer.enabled = visible;

        if (gameController.unitGrid[x, y] != null)
        {
            gameController.unitGrid[x, y].GetComponent<Renderer>().enabled = visible;
        }

        if (gameController.featureGrid[x, y] != null)
        {
            gameController.featureGrid[x, y].GetComponent<Renderer>().enabled = visible;
        }
    }

    int GetPlayerShrineCount()
    {
        return playerShrineList.Count;
    }

    public void ClaimShrine(GameObject s)
    {
        playerShrineList.Add(s);
        s.GetComponent<MapObjectiveScript>().Claim(this.playerNumber);
        Debug.Log("Shrine Claimed");
    }

    public bool RemoveShrine(GameObject s)
    {
        return playerShrineList.Remove(s);
    }

    public float ShrineDamageBonus()
    {
        return 1.0f + (0.1f * playerShrineList.Count);
    }

    public float ShrineDefenseBonus()
    {
        return 1.0f + (0.1f * playerShrineList.Count);
    }

    public int ShrineCount()
    {
        return playerShrineList.Count;
    }
}
