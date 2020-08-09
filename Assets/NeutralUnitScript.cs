using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutralUnitScript : UnitScript
{
    GameObject target;

    int chaseRange;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {

    }

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


}
