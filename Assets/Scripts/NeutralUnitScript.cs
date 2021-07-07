using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NeutralUnitScript : UnitScript
{
    GameObject target;

    int chaseRange = 5;

    bool FindTarget(UnitScript[] unitList)
    {
        GameObject newTarget = null;
        int newTargetDistance = 9999;   // might need to be increased if the map size gets big enough

        foreach(UnitScript u in unitList)
        {
            int d = u.MapDistance(u.xy(), this.xy());

            if (d <= chaseRange && d <= newTargetDistance)
            {
                newTarget = u.gameObject;
                newTargetDistance = d;
            }
        }

        target = newTarget;

        return true;
    }

    public void AutoMove()
    {
        while (this.movementPoints > 0) {
            // 0 = up, 1 = down, 2 = left, 3 = right

            string[] directions = { "up", "down", "left", "right" };

            int r = UnityEngine.Random.Range(0, 4);

            Vector2Int targetSquare = new Vector2Int(this.xy()[0], this.xy()[1]);

            switch (directions[r]) {
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
                    this.movementPoints = 0;
                    break;
            }

            if (SelectDestinationAndMove(targetSquare) != 2)    // Try to move to that square
            {
                // If move failed (square was blocked/inaccessible), zero out move points so the unit doesn't move again
                // Note: sometimes unit will have other valid moves but this will happen anyway. Ah well! This will be fixed when neutral units get better AI.
                this.movementPoints = 0;
            }
        }
    }
    public void NeutralAttackUnit(UnitScript target)   // This is a mess
    {
        Debug.Log("SHITTY FUNCTION CALLED! FIX NeutralAttackUnit() YOU LAZY BITCH");
        if (target.GetComponent<UnitScript>().GetPlayer() != -1)    // probably won't have to change unless I add multiple neutral factions or something
        {
            if (TryAttackUnit(target))
            {
                target.ReceiveDamage(AttackDamage());
            }
        }
    }

}
