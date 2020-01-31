using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerScript : MonoBehaviour
{
    public List<GameObject> playerUnits;
    private GameObject selected;

    public GameObject gameController;
    public GameControllerScript gameControllerScript;
    public GameObject uiController;
    public UIControllerScript uiScript;

    public List<GameObject> terrainGrid;
    public List<GameObject> playerUnitList;
    public List<GameObject> allUnitList;

    int playerNumber;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    GameObject selectUnit(GameObject u)
    {
        selected = u;

        uiScript.ShowUnitInfo(u);

        return u;
    }
    GameObject selectTown(GameObject t)
    {
        selected = t;

        uiScript.ShowTownInfo(t);

        return t;
    }

    GameObject townRecruit(GameObject t)
    {
        TownScript townScript = t.GetComponent<TownScript>();

        int x = townScript.mapX;
        int y = townScript.mapY - 1;

        GameObject newUnit = null;

        if (gameControllerScript.terrainGrid[x, y] != null)
        {
            newUnit = townScript.CreateUnit();
            if (newUnit != null)
            {
                allUnitList.Add(newUnit);
                gameControllerScript.terrainGrid[x, y] = newUnit;
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

        uiScript.ShowTownInfo(t);

        return newUnit;
    }

    public GameObject getPlayerSelection(GameObject s)
    {
        selected = s;

        if (Input.GetMouseButtonDown(0))
        {

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), new Vector3(0, 0, 1));

            if (hit)
            {
                if (hit.transform.gameObject.tag == "Unit")
                {
                    selectUnit(hit.transform.gameObject);
                }
                else if (hit.transform.gameObject.tag == "Town")
                {
                    if(hit.transform.gameObject == selected)
                    {
                        townRecruit(hit.transform.gameObject);
                    } else
                    {
                        selectTown(hit.transform.gameObject);
                    }

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
                            gameControllerScript.MoveUnit(selected, hit.transform.gameObject.GetComponent<TileScript>().mapX, hit.transform.gameObject.GetComponent<TileScript>().mapY);
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

    public void EndTurn()
    {
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
            gameControllerScript.currPlayer = (gameControllerScript.currPlayer + 1) % gameControllerScript.numPlayers;
            uiScript.SetCurrPlayer(gameControllerScript.currPlayer);
        }
    }

    public int[] mapToScreenCoordinates(int x, int y)
    {
        int[] mapDimensions = gameControllerScript.GetMapDimensions();

        int a = x - mapDimensions[1] / 2;
        int b = -y + mapDimensions[0] / 2;

        return new int[] { a, b };
    }
}
