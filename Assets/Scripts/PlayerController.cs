using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    WorldManager worldManager;
    public Player player;

    public UIManager uiManager;

    // Start is called before the first frame update
    void Start()
    {
        worldManager = GameObject.Find("WorldManager").GetComponent<WorldManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool MoveUnit(GameObject u, Vector2Int location)
    {
        return worldManager.MoveUnit(u, location.x, location.y);
    }

    public GameObject getPlayerSelection(GameObject s)
    {
        GameObject selected = s;    // Necessary?

        if (Input.GetMouseButton(1) && worldManager.gameOver == false)
        {
            selected = null;
            uiManager.SetSelectedObject(null);
        }

        if (Input.GetMouseButtonDown(0) && worldManager.gameOver == false)
        {

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), new Vector3(0, 0, 1));

            if (hit)
            {
                string hitTag = hit.transform.gameObject.tag;

                switch (hitTag)
                {
                    case "Unit":
                        if (selected)
                        {
                            if (selected.tag == "Unit")
                            {
                                UnitScript target = hit.transform.gameObject.GetComponent<UnitScript>();

                                // check if we can attack the unit
                                if (target.GetPlayer() != player.playerNumber)
                                {
                                    UnitScript attacker = selected.GetComponent<UnitScript>();
                                    player.AttackUnit(attacker, target);
                                }
                                else
                                {
                                    // select the unit
                                    selected = SelectUnit(hit.transform.gameObject);
                                }
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
                                TownScript target = hit.transform.gameObject.GetComponent<TownScript>();

                                // check if we can attack the town
                                if (target.GetPlayer() != player.playerNumber)
                                {
                                    UnitScript attacker = selected.GetComponent<UnitScript>();
                                    player.AttackTown(attacker, target);
                                }
                                else
                                {
                                    selected = SelectTown(hit.transform.gameObject);
                                }

                            }
                            else
                            {

                                selected = SelectTown(hit.transform.gameObject);
                            }
                        }
                        else
                        {

                            selected = SelectTown(hit.transform.gameObject);
                        }
                        break;
                    case "Terrain":
                        if (selected == null)
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
                        else if (selected.tag == "Unit")
                        {
                            int x = hit.transform.gameObject.GetComponent<TileScript>().mapX;
                            int y = hit.transform.gameObject.GetComponent<TileScript>().mapY;
                            if (worldManager.Walkable(x, y))
                            {
                                worldManager.MoveUnit(selected, hit.transform.gameObject.GetComponent<TileScript>().mapX, hit.transform.gameObject.GetComponent<TileScript>().mapY);
                            }
                        }
                        else if (selected.tag == "MapFeature")
                        {
                            int x = hit.transform.gameObject.GetComponent<MapFeatureScript>().mapX;
                            int y = hit.transform.gameObject.GetComponent<MapFeatureScript>().mapY;
                            if (worldManager.Walkable(x, y))
                            {
                                worldManager.MoveUnit(selected, hit.transform.gameObject.GetComponent<TileScript>().mapX, hit.transform.gameObject.GetComponent<TileScript>().mapY);
                            }
                        }
                        else if (selected.tag == "MapObjective")
                        {
                            int x = hit.transform.gameObject.GetComponent<MapObjectiveScript>().mapX;
                            int y = hit.transform.gameObject.GetComponent<MapObjectiveScript>().mapY;
                            if (worldManager.Walkable(x, y))
                            {
                                worldManager.MoveUnit(selected, hit.transform.gameObject.GetComponent<TileScript>().mapX, hit.transform.gameObject.GetComponent<TileScript>().mapY);
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

        return selected;
    }

    // shouldn't be public
    public GameObject SelectUnit(GameObject u)
    {
        if (u.GetComponent<UnitScript>().GetPlayer() == player.playerNumber)
        {
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
    
    //public GameObject SelectFirstUnitWithMoves()
    //{
    //    foreach (GameObject u in player.playerUnitList)
    //    {
    //        if (u.GetComponent<UnitScript>().GetMovePoints() > 0)
    //        {
    //            mainCamera.transform.position = new Vector3(u.transform.position.x, u.transform.position.y, mainCamera.transform.position.z);
    //            worldManager.Select(u);
    //            SelectUnit(u);

    //            return u;
    //        }
    //    }
    //    return null;
    //}
}
