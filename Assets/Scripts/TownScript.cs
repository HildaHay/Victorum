using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TownScript : GameEntity
{
    [SerializeField] int maxHP;
    int HP;

    public int mapX;
    public int mapY;

    [SerializeField] int baseResourceCollectionRange = 2;

    // public int goldPerTurn;
    // int gold;

    // public GameObject knightPrefab;
    // public GameObject scoutPrefab;

    public GameObject[] recruitableUnits;

    public Sprite playerTownSprite;
    public Sprite enemyTownSprite;
    public SpriteRenderer sRenderer;

    // public bool isPlayer;
    public int playerNumber;

    public GameObject worldManagerObject;
    WorldManager worldManager;

    public GameObject playerControllerObject;
    Player player;

    public GameObject uiControllerObject;
    UIManager uiController;

    // Start is called before the first frame update
    void Start()
    {
        worldManager = worldManagerObject.GetComponent<WorldManager>();

        player = playerControllerObject.GetComponent<Player>();

        uiController = uiControllerObject.GetComponent<UIManager>();

        // recruitableUnits = new GameObject[] { knightPrefab, scoutPrefab };

        // gold = 10;

        HP = maxHP;

        sRenderer = gameObject.GetComponent<SpriteRenderer>();

        if(playerNumber == 0)
        {
            sRenderer.sprite = playerTownSprite;
        } else
        {
            sRenderer.sprite = enemyTownSprite;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnSelect()
    {
        GameObject[] tilesToHighlight = GetOwnedTiles();
        foreach(GameObject t in tilesToHighlight)
        {
            t.GetComponent<SpriteRenderer>().color = GetPlayerColor();
        }
    }

    public override void OnDeselect()
    {
        GameObject[] tilesToHighlight = GetOwnedTiles();
        foreach (GameObject t in tilesToHighlight)
        {
            t.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    public void TurnStart()
    {
        // gold += goldPerTurn;
    }

    public bool CanBuy()
    {
        return player.Gold() > 0;
    }

    public int GetPlayerGold()
    {
        return player.Gold();
    }

    public int GetPlayer()
    {
        return playerNumber;
    }

    public Color GetPlayerColor()
    {
        if(playerNumber == 0)
        {
            //return Color.green;
            return new Color(2.0f / 256.0f, 218.0f / 256.0f, 136.0f / 256.0f);
        }
        else
        {
            return Color.red;
        } 
    }

    public int GetCollectionRage()
    {
        return baseResourceCollectionRange;
    }

    public void OpenMenu()
    {
        uiController.OpenTownMenu(recruitableUnits);
    }

    public void OrderBuildUnit(GameObject unitToBuild)
    {
        player.TownRecruit(this.gameObject, unitToBuild);
    }

    public GameObject BuildUnit(GameObject unitPrefab)
    {
        // actual unit spawning handled by worldManagerScript

        if(player.Gold() >= unitPrefab.GetComponent<UnitScript>().cost)
        {
            player.RemoveGold(unitPrefab.GetComponent<UnitScript>().cost);

            return worldManager.SpawnPlayerUnit(unitPrefab, worldManager.playerObjects[playerNumber].GetComponent<Player>());
        }
        else
        {
            Debug.Log("Out of gold");
            return null;
        }
    }

    public bool ReceiveDamage(float d)
    {
        float rawDamage = d / player.ShrineDefenseBonus();

        int roundedDamage = Mathf.RoundToInt(rawDamage);

        HP -= System.Math.Max(roundedDamage, 0);

        if (HP <= 0)
        {
            Die();
            return true;
        }
        else
        {
            return false;
        }
    }

    bool Die()
    {
        worldManager.DeleteTown(this.gameObject);
        return true;
    }

    public Vector2Int xy()
    {
        return new Vector2Int ( mapX, mapY );
    }

    public Vector2Int GetLocation()
    {
        return new Vector2Int(mapX, mapY);
    }

    // Get a list of tiles that this town "owns"
    public GameObject[] GetOwnedTiles()
    {
        GameObject[] tiles = new GameObject[(baseResourceCollectionRange * 2 + 1) * (baseResourceCollectionRange * 2 + 1)];

        // Note: this will crash if the town is too close to the edge of the map!
        // TODO: Fix this!!!
        int i = 0;
        for(int x = mapX - baseResourceCollectionRange; x <= mapX + baseResourceCollectionRange; x++)
        {
            for(int y = mapY - baseResourceCollectionRange; y <= mapY + baseResourceCollectionRange; y++)
            {
                tiles[i] = worldManager.terrainGrid[x, y];
                i++;
            }
        }

        return tiles;
    }

    // gets all MapFeatures within the town's orders
    public List<GameObject> GetOwnedFeatures()
    {
        Debug.Log("GetOwnedFeatures called");   // don't want to call this too often since it probably takes a while
        List<GameObject> features = new List<GameObject>();

        // Note: this will crash if the town is too close to the edge of the map!
        // TODO: Fix this!!!
        for (int x = mapX - baseResourceCollectionRange; x <= mapX + baseResourceCollectionRange; x++)
        {
            for (int y = mapY - baseResourceCollectionRange; y <= mapY + baseResourceCollectionRange; y++)
            {
                if (worldManager.featureGrid[x, y] != null)
                {
                    features.Add(worldManager.featureGrid[x, y]);
                }
            }
        }

        return features;
    }

    public int GoldYield()
    {
        int g = 0;

        List<GameObject> features = GetOwnedFeatures();
        foreach(GameObject f in features)
        {
            MapFeatureScript s = f.GetComponent<MapFeatureScript>();
            if(s != null)   // This is necessary because featureGrid can contain both features and objectives
                            // it's a messy solution that should be fixed, probably by making MapObjective inherit from MapFeature
            {
                g += s.resourceYield;
            }
        }

        return g;
    }

    //public GameObject CreateUnit()
    //{
    //    if(gold >= 1)
    //    {
    //        gold -= 1;
    //        GameObject newUnit = Instantiate(unitPrefab, new Vector3(0, 0, -1), Quaternion.identity);
    //        return newUnit;
    //    }
    //    else
    //    {
    //        Debug.Log("Out of gold");
    //        return null;
    //    }
    //}
}
