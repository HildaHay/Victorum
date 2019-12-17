using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightScript : MonoBehaviour
{
    // Start is called before the first frame update

    int maxMovement;
    int movementPoints;

    public int mapX;
    public int mapY;
    void Start()
    {
        maxMovement = 3;
        movementPoints = maxMovement;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("r"))
        {
            movementPoints = maxMovement;
        }
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

    public bool CanMove()
    {
        return movementPoints > 0;
    }

    public int getMovePoints()
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
