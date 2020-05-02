using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightScript : MonoBehaviour
{
    // Start is called before the first frame update

    int maxMovement = 3;
    int movementPoints;

    int maxHP = 10;
    int HP;

    int baseDamage = 2;
    int defense = 0;

    public int player;

    static string unitName = "Knight";

    public int mapX;
    public int mapY;

    GameControllerScript gameController;

    void Start()
    {
        // maxMovement = 3;
        movementPoints = maxMovement;

        HP = maxHP;
    }

    public void Initialize(int p, GameControllerScript gC)
    {
        player = p;
        gameController = gC;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("r"))
        {
            movementPoints = maxMovement;
        }
    }

    public bool tryAttack()
    {
        if(movementPoints > 0)
        {
            movementPoints--;
            return true;
        } else
        {
            Debug.Log("Out of movement.");
            return false;
        }
    }

    public int attackDamage()
    {
        return baseDamage;
    }

    public bool receiveDamage(int d) {
        HP -= System.Math.Max(d - defense, 0);

        if(HP <= 0)
        {
            die();
            return true;
        } else
        {
            return false;
        }
    }

    bool die()
    {
        gameController.DeleteUnit(this.gameObject);
        return true;
    }

    public bool TryMove(int x)
    {
        if(movementPoints < x)
        {
            print("not enough movement points");
            return false;
        } else
        {
            movementPoints -= x;
            return true;
        }
    }

    public string GetName()
    {
        return unitName;
    }

    public int GetPlayer()
    {
        return player;
    }

    public int SetPlayer(int p)
    {
        player = p;
        return player;
    }

    public bool CanMove()
    {
        return movementPoints > 0;
    }

    public int GetMovePoints()
    {
        return movementPoints;
    }

    public void ResetMovePoints()
    {
        movementPoints = maxMovement;
    }

    public int[] xy()
    {
        return new int[] { mapX, mapY };
    }
}
