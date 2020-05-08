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

    public int goldPerTurn;
    int gold;

    public GameObject unitPrefab;

    public Sprite playerTownSprite;
    public Sprite enemyTownSprite;
    public SpriteRenderer sRenderer;

    // public bool isPlayer;
    public int player;

    public GameObject gameControllerObject;
    GameControllerScript gameController;

    public GameObject playerControllerObject;
    PlayerControllerScript playerController;

    public GameObject uiControllerObject;
    UIControllerScript uiController;

    // Start is called before the first frame update
    void Start()
    {
        gameController = gameControllerObject.GetComponent<GameControllerScript>();

        playerController = playerControllerObject.GetComponent<PlayerControllerScript>();

        uiController = uiControllerObject.GetComponent<UIControllerScript>();

        gold = 0;

        HP = maxHP;

        sRenderer = gameObject.GetComponent<SpriteRenderer>();

        if(player == 0)
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
        gold += goldPerTurn;
    }

    public bool CanBuy()
    {
        return gold > 0;
    }

    public int GetGold()
    {
        return gold;
    }

    public int GetPlayer()
    {
        return player;
    }

    public void OpenMenu()
    {
        uiController.OpenTownMenu();
    }

    public void OrderBuildUnit()
    {
        playerController.TownRecruit(this.gameObject);
    }

    public GameObject BuildUnit()
    {
        // actual unit spawning handled by GameControllerScript

        if (gold >= 1)
        {
            gold -= 1;

            return gameController.SpawnUnit(unitPrefab, player);
        }
        else
        {
            Debug.Log("Out of gold");
            return null;
        }
    }

    public bool receiveDamage(int d)
    {
        HP -= System.Math.Max(d /* - defense*/, 0);

        if (HP <= 0)
        {
            die();
            return true;
        }
        else
        {
            return false;
        }
    }

    bool die()
    {
        gameController.DeleteTown(this.gameObject);
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
