using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TownScript : MonoBehaviour
{
    public int maxHP;
    int HP;

    public int mapX;
    public int mapY;

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

    public void TurnStart()
    {
        // gold += goldPerTurn;
    }

    public bool CanBuy()
    {
        return player.gold > 0;
    }

    public int GetGold()
    {
        return player.gold;
    }

    public int GetPlayer()
    {
        return playerNumber;
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

        if(player.gold >= unitPrefab.GetComponent<UnitScript>().cost)
        {
            player.gold -= unitPrefab.GetComponent<UnitScript>().cost;

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

    public int[] xy()
    {
        return new int[] { mapX, mapY };
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
