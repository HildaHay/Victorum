using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TownScript : MonoBehaviour
{
    public int mapX;
    public int mapY;

    public int gold;
    public int goldPerTurn;

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

        goldPerTurn = 1;
        gold = 0;

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
        Debug.Log("goldadd");
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
