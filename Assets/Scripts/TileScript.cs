using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    public int mapX;
    public int mapY;

    public bool walkable;

    public Renderer tileRenderer;

    // Start is called before the first frame update
    void Start()
    {
        tileRenderer = this.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
