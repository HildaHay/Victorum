using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayerController : PlayerController
{
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
    }
}
