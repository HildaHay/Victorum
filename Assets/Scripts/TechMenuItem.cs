using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechMenuItem : MonoBehaviour
{
    //int techID;
    //string techName;
    public Tech tech;

    // Start is called before the first frame update
    void Start()
    {
        Text buttonText = transform.GetChild(0).gameObject.GetComponent<Text>();
        buttonText.text = tech.Name();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
