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

        if(u.GetComponent<KnightScript>().GetPlayer() == playerNumber)
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
        if (t.GetComponent<TownScript>().GetPlayer() == playerNumber)
        {
            uiController.SetSelectedObject(t);

            return t;
        } else
        {
            return null;
        }
    }

    GameObject TownRecruit(GameObject t)
    {
        TownScript townScript = t.GetComponent<TownScript>();

        int x = townScript.mapX;
        int y = townScript.mapY - 1;

        GameObject newUnit = null;

        if (gameController.unitGrid[x, y] == null)
        {
            newUnit = townScript.BuildUnit();
            if (newUnit != null)
            {
                gameController.unitList.Add(newUnit);
                gameController.unitGrid[x, y] = newUnit;
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

        // uiScript.SetSelectedObject(t);

        return newUnit;
    }

    public void combat(KnightScript attacker, KnightScript defender)
    {
        int aDmg = attacker.attackDamage();
        int dDmg = defender.attackDamage();

        defender.receiveDamage(aDmg);
        attacker.receiveDamage(dDmg);
    }

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
                            KnightScript attacker = selected.GetComponent<KnightScript>();
                            KnightScript target = hit.transform.gameObject.GetComponent<KnightScript>();
                            if (attacker.GetComponent<KnightScript>().player == playerNumber
                                && target.GetComponent<KnightScript>().player != playerNumber)
                            {
                                if(attacker.tryAttack())
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
                    /*if(hit.transform.gameObject == selected)
                    {
                        selected = townRecruit(hit.transform.gameObject);
                    } else
                    {
                        selected = selectTown(hit.transform.gameObject);
                    }*/

                    selected = selectTown(hit.transform.gameObject);
                    selected.GetComponent<TownScript>().OpenMenu();

                }
                else if (hit.transform.gameObject.tag == "Terrain")
                {
                    if(selected == null)
                    {

                    }
                    else if (selected.tag == "Unit")
                    {
                        if (hit.transform.gameObject.GetComponent<TileScript>().walkable)
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
    }

    public void EndTurn()
    {
        uiController.SetSelectedObject(null);

        foreach(GameObject u in playerUnitList)
        {
            if(u.tag == "Unit")
            {
                u.GetComponent<KnightScript>().ResetMovePoints();
            }
            if(u.tag == "Town")
            {
                u.GetComponent<TownScript>().TurnStart();
            }
        }
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
}
