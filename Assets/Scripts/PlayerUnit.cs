using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : UnitScript
{
    // Start is called before the first frame update

    // int playerNumber;

    // private PlayerControllerScript player;

    //void Start()
    //{
    //    base.Start();
    //}

    // Update is called once per frame
    void Update()
    {

    }

    public new float AttackDamage()
    {
        float baseDamage = UnityEngine.Random.Range((float)minDamage - 0.49f, (float)maxDamage + 0.49f);
        // the 0.49 should make the damage outputs be randomly distributed over the range after being rounded
        // Otherwise, the distribution will be biased against the min and max values

        // return baseDamage * player.ShrineDamageBonus();
        return baseDamage * player.ShrineDamageBonus();
    }

    public new bool ReceiveDamage(float d)
    {
        // float rawDamage = d / player.ShrineDefenseBonus() - armor;
        float rawDamage = d - armor;

        print(rawDamage);

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
}
