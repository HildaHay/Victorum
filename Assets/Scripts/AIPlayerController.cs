using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayerController : PlayerController
{
    public GameObject scoutPrefab;  // temp for dev - remove

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override bool IsHuman()
    {
        return false;
    }

    public override GameObject NextUnit()
    {
        // This function is supposed to get the first unmoved unit for human players
        // For an AI player, it is useless
        return null;
    }

    public override void OnTurnStart()
    {
        base.OnTurnStart();
        AITakeTurn();
    }

    // Handles everything the AI does on its turn
    public void AITakeTurn()
    {
        foreach (GameObject town in player.TownList())
        {
            if (player.gold > 10)    // cost of a Scout
            {
                player.TownRecruit(town, scoutPrefab);
            }
        }

        foreach (GameObject unit in player.UnitList())
        {
            // Note: If it becomes possible for units to die while attacking, this enumerator will break
            UnitScript s = unit.GetComponent<UnitScript>();
            if (s.GetMovePoints() > 0)
            {
                MoveUnitRandomly(s);
            }
        }

        worldManager.EndTurn();
    }

    void MoveUnitRandomly(UnitScript s)
    {
        // Moves the unit in random directions until it runs out of movement or hits an obstacle
        while (s.GetMovePoints() > 0)
        {
            Vector2Int startingPosition = s.xy();

            // 0 = up, 1 = down, 2 = left, 3 = right

            string[] directions = { "up", "down", "left", "right" };

            int r = Random.Range(0, 4);

            Vector2Int targetSquare = startingPosition;

            switch (directions[r])
            {
                case "up":
                    targetSquare.y -= 1;   // Should check if the Y-values for up/down are correct
                    break;
                case "down":
                    targetSquare.y += 1;
                    break;
                case "left":
                    targetSquare.x -= 1;
                    break;
                case "right":
                    targetSquare.x += 1;
                    break;
                default:
                    s.ZeroMovePoints();
                    break;
            }



            if (worldManager.unitGrid[targetSquare.x, targetSquare.y] != null)
            {

                if (worldManager.unitGrid[targetSquare.x, targetSquare.y].tag == "Unit") {
                    player.AttackUnit(s, worldManager.unitGrid[targetSquare.x, targetSquare.y].GetComponent<UnitScript>());
                }
                else
                if (worldManager.unitGrid[targetSquare.x, targetSquare.y].tag == "Town")
                {
                    player.AttackTown(s, worldManager.unitGrid[targetSquare.x, targetSquare.y].GetComponent<TownScript>());
                }
                else
                {
                    s.ZeroMovePoints();
                }
            }
            else
            {
                // Try to move the unit
                if (s.SelectDestinationAndMove(targetSquare) != 2)
                {
                    // If the unit can't move in that direction, end its movement entirely
                    s.ZeroMovePoints();
                }
            }
        }
    }
}