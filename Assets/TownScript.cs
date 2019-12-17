using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownScript : MonoBehaviour
{
    public int mapX;
    public int mapY;

    public int gold;
    public int goldPerTurn;

    public GameObject unitPrefab;

    // Start is called before the first frame update
    void Start()
    {
        goldPerTurn = 1;
        gold = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TurnStart()
    {
        gold += goldPerTurn;
    }

    public bool CanBuy()
    {
        return gold > 0;
    }

    public GameObject CreateUnit()
    {
        if(gold >= 1)
        {
            gold -= 1;
            GameObject newUnit = Instantiate(unitPrefab, new Vector3(0, 0, -1), Quaternion.identity);
            return newUnit;
        }
        else
        {
            Debug.Log("Out of gold");
            return null;
        }
    }
}
