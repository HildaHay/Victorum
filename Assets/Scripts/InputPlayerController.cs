using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputPlayerController : PlayerController
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
                                selectedObject.GetComponent<UnitScript>().SelectDestination(hit.transform.gameObject.GetComponent<TileScript>().mapX, hit.transform.gameObject.GetComponent<TileScript>().mapY);
                            }
                        }
                        else if (selectedObject.tag == "MapFeature")
                        {
                            int x = hit.transform.gameObject.GetComponent<MapFeatureScript>().mapX;
                            int y = hit.transform.gameObject.GetComponent<MapFeatureScript>().mapY;
                            if (worldManager.Walkable(x, y))
                            {
                                selectedObject.GetComponent<UnitScript>().SelectDestination(hit.transform.gameObject.GetComponent<TileScript>().mapX, hit.transform.gameObject.GetComponent<TileScript>().mapY);
                            }
                        }
                        else if (selectedObject.tag == "MapObjective")
                        {
                            int x = hit.transform.gameObject.GetComponent<MapObjectiveScript>().mapX;
                            int y = hit.transform.gameObject.GetComponent<MapObjectiveScript>().mapY;
                            if (worldManager.Walkable(x, y))
                            {
                                selectedObject.GetComponent<UnitScript>().SelectDestination(hit.transform.gameObject.GetComponent<TileScript>().mapX, hit.transform.gameObject.GetComponent<TileScript>().mapY);
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
}
