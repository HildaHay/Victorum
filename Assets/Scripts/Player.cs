using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    // GameObject selected;

    WorldManager worldManager;
    UIManager uiController;

    GameObject controllerObject;
    [SerializeField] PlayerController controller; // shouldn't be public

    protected List<GameObject> playerUnitList;
    protected List<GameObject> playerTownList;
    protected List<GameObject> playerObjectiveList;

    public bool[,] tilesExplored;

    GameObject mainTown;

    public int playerNumber;

    Camera mainCamera;

    Vector3 playerCameraPosition;

    public bool playerActive;

    // int unitVisionDistance = 3;
    int townVisionDistance = 6;

    [SerializeField] int gold;
    [SerializeField] int baseGPT; // base gold per turn
    [SerializeField] int townGPT; // additional gold per turn for each town constructed
    [SerializeField] int mineGPT; // additional gold per turn for each mine controlled

    [SerializeField] int baseSciencePerTurn;

    [SerializeField] GameObject TechTreePrefab;
    GameObject TechTreeObject;
    TechTreeScript TechTree;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main; //y tho?

        playerCameraPosition = mainTown.transform.position + new Vector3(0, 0, -10);

        playerObjectiveList = new List<GameObject>();

        // controllerObject = Instantiate(worldManager.playerControllerPrefab, this.gameObject.transform);
        controller = controllerObject.GetComponent<PlayerController>();
        controller.player = this;
        controller.uiManager = uiController;
        controller.worldManager = worldManager;

        TechTreeObject = Instantiate(TechTreePrefab);
        TechTree = TechTreeObject.GetComponent<TechTreeScript>();
        TechTree.player = this;
        TechTree.Initialize();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Initialize(int p, WorldManager wm, UIManager uiC, bool isHuman)
    {
        playerNumber = p;
        worldManager = wm;
        uiController = uiC;

        playerActive = true;

        playerUnitList = new List<GameObject>();
        playerTownList = new List<GameObject>();

        tilesExplored = new bool[worldManager.getMapDimensions()[0], worldManager.getMapDimensions()[1]];

        gold = 20;
        baseGPT = 2;
        townGPT = 0;
        mineGPT = 1;

        baseSciencePerTurn = 10;

        if(isHuman) {
            controllerObject = Instantiate(worldManager.humanPlayerControllerPrefab, this.gameObject.transform);
        } else
        {
            controllerObject = Instantiate(worldManager.aiPlayerControllerPrefab, this.gameObject.transform);
        }
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
                if (i >= 0 && j >= 0 && i < tilesExplored.GetLength(0) && j < tilesExplored.GetLength(1))
                {
                    if (Math.Abs(x - i) + Math.Abs(y - j) < visionRange)
                    {
                        tilesExplored[i, j] = true;
                        SetTileVisibility(i, j, tilesExplored[i, j]);
                    }
                }
            }
        }
    }

    public GameObject TownRecruit(GameObject t, GameObject unitToBuild)
    {
        TownScript townScript = t.GetComponent<TownScript>();

        int x = townScript.mapX;
        int y = townScript.mapY + 1;

        GameObject newUnit = null;

        if (worldManager.unitGrid[x, y] == null)
        {
            newUnit = townScript.BuildUnit(unitToBuild);
            if (newUnit != null)
            {
                // worldManager.unitList.Add(newUnit);
                worldManager.unitGrid[x, y] = newUnit;
                newUnit.GetComponent<UnitScript>().mapX = x;
                newUnit.GetComponent<UnitScript>().mapY = y;

                int[] screenCoords = mapToScreenCoordinates(x, y);
                newUnit.transform.position = new Vector3(screenCoords[0], screenCoords[1], -1);
            }
        }
        else
        {
            //Debug.Log("Space occupied");
        }

        uiController.SetSelectedObject(t);

        return newUnit;
    }

    public void AttackTown(UnitScript attacker, TownScript target)
    {
        if (attacker.TryAttackTown(target))
        {
            target.ReceiveDamage(attacker.AttackDamage());
        } else
        {
            return;
        }

    }

    public void AttackUnit(UnitScript attacker, UnitScript target)  // should be moved to unit script! or something
    {
        while (attacker.GetMovePoints() > 0)
        {
            if (attacker.TryAttackUnit(target))
            {
                target.ReceiveDamage(attacker.AttackDamage());
            } else
            {
                return;
            }
        }
    }

    public void StartTurn()
    {
        mainCamera.transform.position = playerCameraPosition;

        foreach (GameObject u in playerUnitList)
        {
            u.GetComponent<UnitScript>().ResetMovePoints();
        }
        foreach (GameObject t in playerTownList)
        {
            TownScript s = t.GetComponent<TownScript>();
            s.TurnStart();
            CheckVision(s.mapX, s.mapY, townVisionDistance);
        }

        AddGPT();
        AdvanceResearch();

        ShowExplored();

        controller.OnTurnStart();
    }

    public void EndOfTurnCleanup()
    {
        HideAll();

        playerCameraPosition = mainCamera.transform.position;

        uiController.SetSelectedObject(null);
    }

    public int GoldPerTurn()
    {
        int g = baseGPT;
        g += mineGPT * GetPlayerMineCount();
        foreach(GameObject t in TownList())
        {
            g += t.GetComponent<TownScript>().GoldYield();
        }
        g += townGPT * playerTownList.Count;

        return g;
    }

    public void AddGPT()
    {
        gold += GoldPerTurn();
    }

    public void AdvanceResearch()
    {
        TechTree.ProgressResearch(baseSciencePerTurn);
    }

    public int[] mapToScreenCoordinates(int x, int y)
    {
        Vector2Int mapDimensions = worldManager.GetMapDimensions();

        int a = x - mapDimensions.y / 2;
        int b = -y + mapDimensions.x / 2;

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

    public GameObject SetMainTown(GameObject t)
    {
        mainTown = t;
        return mainTown;
    }

    public GameObject GetMainTown()
    {
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

        if (playerTownList.Count <= 0)
        {
            playerActive = false;
            worldManager.CheckForWinner();
        }

        return true;
    }

    void HideAll()
    {
        GameObject[,] terrainGrid = worldManager.terrainGrid;

        for (int i = 0; i < terrainGrid.GetLength(0); i++)
        {
            for (int j = 0; j < terrainGrid.GetLength(1); j++)
            {
                SetTileVisibility(i, j, false);
            }
        }
    }
    void ShowAll()
    {
        for (int i = 0; i < worldManager.terrainGrid.GetLength(0); i++)
        {
            for (int j = 0; j < worldManager.terrainGrid.GetLength(1); j++)
            {
                SetTileVisibility(i, j, true);
            }
        }
    }

    void ShowExplored()
    {
        for (int i = 0; i < worldManager.terrainGrid.GetLength(0); i++)
        {
            for (int j = 0; j < worldManager.terrainGrid.GetLength(1); j++)
            {
                SetTileVisibility(i, j, tilesExplored[i, j]);
            }
        }
    }

    void SetTileVisibility(int x, int y, bool visible)
    {
        if (worldManager.debugNoFoW)
        {
            visible = true;
        }

        worldManager.terrainGrid[x, y].GetComponent<TileScript>().tileRenderer.enabled = visible;

        if (worldManager.unitGrid[x, y] != null)
        {
            // worldManager.unitGrid[x, y].GetComponent<Renderer>().enabled = visible;
            Renderer[] renderers = worldManager.unitGrid[x, y].GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.enabled = visible;
            }
        }

        if (worldManager.featureGrid[x, y] != null)
        {
            worldManager.featureGrid[x, y].GetComponent<Renderer>().enabled = visible;
        }
    }

    int GetPlayerShrineCount()
    {
        return playerObjectiveList.Count;
    }

    int GetStrengthShrineCount()
    {
        int c = 0;
        foreach (GameObject o in playerObjectiveList)
        {
            if (o.GetComponent<MapObjectiveScript>().objectiveType == 1)
            {
                c++;
            }
        }
        return c;
    }

    int GetDefenseShrineCount()
    {
        int c = 0;
        foreach (GameObject o in playerObjectiveList)
        {
            if (o.GetComponent<MapObjectiveScript>().objectiveType == 2)
            {
                c++;
            }
        }
        return c;
    }

    int GetPlayerMineCount()
    {
        int c = 0;
        foreach (GameObject o in playerObjectiveList)
        {
            if (o.GetComponent<MapObjectiveScript>().objectiveType == 0)
            {
                c++;
            }
        }
        return c;
    }

    public void AddShrine(GameObject s)
    {
        playerObjectiveList.Add(s);
        s.GetComponent<MapObjectiveScript>().Claim(this.playerNumber);
    }

    public bool RemoveShrine(GameObject s)
    {
        return playerObjectiveList.Remove(s);
    }

    public float ShrineDamageBonus()
    {
        return 1.0f + (0.1f * GetStrengthShrineCount());
    }

    public float ShrineDefenseBonus()
    {
        return 1.0f + (0.1f * GetDefenseShrineCount());
    }

    public int ShrineCount()
    {
        return playerObjectiveList.Count;
    }

    public GameObject NextUnit()
    {
        return controller.NextUnit();
    }

    public List<GameObject> UnitList()
    {
        return playerUnitList;
    }

    public List<GameObject> TownList()
    {
        return playerTownList;
    }

    public int Gold()
    {
        return gold;
    }

    public int AddGold(int a)
    {
        gold += a;
        return gold;
    }

    public int RemoveGold(int r)
    {
        gold -= r;
        return gold;
    }

    public bool TechResearched(String techName)
    {
        return true;
    }

    public bool IsHuman()
    {
        return false;
    }
}
