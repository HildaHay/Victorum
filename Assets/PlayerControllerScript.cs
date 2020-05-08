using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerControllerScript : MonoBehaviour
{
    // GameObject selected;

    GameControllerScript gameController;
    UIControllerScript uiController;

    List<GameObject> playerUnitList;
    GameObject mainTown;

    int playerNumber;

    Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
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

        playerUnitList = new List<GameObject>();
    }

    GameObject selectUnit(GameObject u)
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
    GameObject selectTown(GameObject t)
    {
        uiController.SetSelectedObject(t);
        return t;
    }

    public GameObject TownRecruit(GameObject t)
    {
        TownScript townScript = t.GetComponent<TownScript>();

        int x = townScript.mapX;
        int y = townScript.mapY + 1;

        print(x + ", " + y);

        GameObject newUnit = null;

        if (gameController.unitGrid[x, y] == null)
        {
            newUnit = townScript.BuildUnit();
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

        if (Input.GetMouseButtonDown(0))
        {

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), new Vector3(0, 0, 1));

            if (hit)
            {
                if (hit.transform.gameObject.tag == "Unit")
                {
                    if (selected)
                    {
                        // check if we can attack the unit
                        if (selected.tag == "Unit")
                        {
                            UnitScript attacker = selected.GetComponent<UnitScript>();
                            UnitScript target = hit.transform.gameObject.GetComponent<UnitScript>();
                            if (attacker.GetComponent<UnitScript>().GetPlayer() == playerNumber
                                && target.GetComponent<UnitScript>().GetPlayer() != playerNumber)
                            {
                                if (attacker.TryAttack(target))
                                {
                                    target.receiveDamage(attacker.attackDamage());
                                }
                            }
                            else
                            {
                                // select the unit
                                selected = selectUnit(hit.transform.gameObject);
                            }
                        }
                        else
                        {
                            // select the unit
                            selected = selectUnit(hit.transform.gameObject);
                        }
                    }
                    else
                    {
                        selected = selectUnit(hit.transform.gameObject);
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
                            if (attacker.GetComponent<UnitScript>().GetPlayer() == playerNumber
                                && target.GetComponent<TownScript>().GetPlayer() != playerNumber)
                            {
                                if (attacker.TryAttackTown(target))
                                {
                                    target.receiveDamage(attacker.attackDamage());
                                }
                            }
                            else
                            {

                                selected = selectTown(hit.transform.gameObject);
                                if (selected.GetComponent<TownScript>().player == playerNumber)
                                {
                                    selected.GetComponent<TownScript>().OpenMenu();
                                }
                            }
                        } else
                        {

                            selected = selectTown(hit.transform.gameObject);
                            if (selected.GetComponent<TownScript>().player == playerNumber)
                            {
                                selected.GetComponent<TownScript>().OpenMenu();
                            }
                        }
                    }
                    else
                    {

                        selected = selectTown(hit.transform.gameObject);
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
                    else
                    {
                        // selected = null;
                    }
                }

                // Do something with the object that was hit by the raycast.
            }
        }

        return selected;
    }

    public void StartTurn()
    {
        mainCamera.transform.position = mainTown.transform.position + new Vector3(0, 0, -10);

        foreach (GameObject u in playerUnitList)
        {
            if (u.tag == "Unit")
            {
                u.GetComponent<UnitScript>().ResetMovePoints();
            }
            if (u.tag == "Town")
            {
                u.GetComponent<TownScript>().TurnStart();
            }
        }
    }

    public void EndTurn()
    {
        uiController.SetSelectedObject(null);
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
        playerUnitList.Remove(t);

        return true;
    }
}
