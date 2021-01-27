using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public WorldManager worldManager;
    public Player player;

    public UIManager uiManager;

    // Start is called before the first frame update
    void Start()
    {
        worldManager = GameObject.Find("WorldManager").GetComponent<WorldManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool MoveUnit(GameObject u, Vector2Int location)
    {
        return u.GetComponent<UnitScript>().SelectDestination(location.x, location.y);
    }

    public virtual GameObject NextUnit() {
        return null;
    }

    public virtual bool IsHuman() {
        return false;
    }
    
    public virtual void OnTurnStart()
    {

    }
}
