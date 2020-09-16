using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObjectiveScript : MonoBehaviour
{
    public int mapX;
    public int mapY;

    public string objectiveName;

    public int player;

    public int objectiveType;

    public Sprite playerObjectiveSprite;
    public Sprite enemyObjectiveSprite;

    public float damageMultiplerBonus;
    public float defenseMultiplierBonus;

    // Start is called before the first frame update
    void Start()
    {
        player = -1;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Claim(int p)
    {
        player = p;

        SpriteRenderer sRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (player == 0)
        {
            sRenderer.sprite = playerObjectiveSprite;
        }
        else
        {
            sRenderer.sprite = enemyObjectiveSprite;
        }
    }
}
