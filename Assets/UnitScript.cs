using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScript : MonoBehaviour
{
    // Start is called before the first frame update

    public int cost;

    public int maxMovement;
    int movementPoints;

    public int maxHP;
    int HP;

    public int baseDamage;
    public int defense;

    public int range;

    int player;

    public string unitName = "";

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

    public bool TryAttack(UnitScript target)
    {
        int distance = (Math.Abs(mapX - target.mapX) + Math.Abs(mapY - target.mapY));
        print(distance);

        if(distance > range) {
            Debug.Log("Out of range");
            return false;
        }
        if(movementPoints <= 0)
        {
            Debug.Log("Out of movement.");
            return false;
        }

        movementPoints--;
        return true;
    }

    public bool TryAttackTown(TownScript target)
    {
        int distance = (Math.Abs(mapX - target.mapX) + Math.Abs(mapY - target.mapY));
        print(distance);

        if (distance > range)
        {
            Debug.Log("Out of range");
            return false;
        }
        if (movementPoints <= 0)
        {
            Debug.Log("Out of movement.");
            return false;
        }

        movementPoints--;
        return true;
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

    /*public bool CanMove()
    {
        return movementPoints > 0;
    }*/

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
