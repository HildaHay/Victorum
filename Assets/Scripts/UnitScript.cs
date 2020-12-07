using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScript : MonoBehaviour
{
    // Start is called before the first frame update

    public int cost;

    public int maxMovement;
    protected int movementPoints;

    public int maxHP;
    public int HP;

    public int minDamage;
    public int maxDamage;

    public int armor;

    public int range;

    public int visionRange;

    protected int playerNumber;

    public string unitName = "";

    public int mapX;
    public int mapY;

    public bool isTownBuilder;

    protected WorldManager worldManager;
    protected Player player;

    public GameObject playerIndicatorSprite;

    public Sprite[] sprites;

    protected void Start()
    {
        // maxMovement = 3;
        movementPoints = maxMovement;

        HP = maxHP;
    }

    public void Initialize(WorldManager wm)
    {
        playerNumber = -1;
        worldManager = wm;
    }

    public void Initialize(Player p, WorldManager wm)
    {
        player = p;
        playerNumber = p.playerNumber;
        worldManager = wm;

        playerIndicatorSprite.GetComponent<SpriteRenderer>().sprite = sprites[p.playerNumber];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool TryAttack(UnitScript target)
    {
        int distance = (Math.Abs(mapX - target.mapX) + Math.Abs(mapY - target.mapY));

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

    public float AttackDamage()
    {
        float baseDamage = UnityEngine.Random.Range((float)minDamage - 0.49f, (float)maxDamage + 0.49f);
        // the 0.49 should make the damage outputs be randomly distributed over the range after being rounded
        // Otherwise, the distribution will be biased against the min and max values

        // return baseDamage * player.ShrineDamageBonus();
        return baseDamage * player.ShrineDamageBonus();
    }

    public bool ReceiveDamage(float d) {
        // float rawDamage = d / player.ShrineDefenseBonus() - armor;
        float rawDamage = 0;

        if (playerNumber == -1)
        {
            // Neutral units get no shrine bonus
            rawDamage = d - armor;
        }
        else
        {
            rawDamage = d / player.ShrineDefenseBonus() - armor;
        }

        print(rawDamage);

        int roundedDamage = Mathf.RoundToInt(rawDamage);


        HP -= System.Math.Max(roundedDamage, 0);

        if(HP <= 0)
        {
            Die();
            return true;
        } else
        {
            return false;
        }
    }

    protected void Die()
    {
        worldManager.DeleteUnit(this.gameObject);
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
        return playerNumber;
    }

    public int SetPlayer(Player p)
    {
        player = p;

        playerNumber = p.playerNumber;
        return playerNumber;
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

    public Vector2Int xy()
    {
        return new Vector2Int (mapX, mapY );
    }

    public int MapDistance(Vector2Int start, Vector2Int end)
    {
        return Math.Abs(start[0] - end[0]) + Math.Abs(start[1] - end[1]);
    }
}
