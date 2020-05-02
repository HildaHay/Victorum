using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIControllerScript : MonoBehaviour
{
    public GameObject selectionTextObject;
    Text selectionText;

    public GameObject currPlayerTextObject;
    Text currPlayerText;

    int currPlayer;
    GameObject selectedObject;

    public GameObject townMenu;

    // Start is called before the first frame update
    void Start()
    {
        selectionText = selectionTextObject.GetComponent<Text>();
        selectionText.text = "";

        currPlayerText = currPlayerTextObject.GetComponent<Text>();
        currPlayerText.text = "";

        townMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(selectedObject == null)
        {
            selectionText.text = "";
        } else { 
            if(selectedObject.tag == "Unit")
            {
                ShowUnitInfo(selectedObject);
            }
            else if (selectedObject.tag == "Town")
            {
                ShowTownInfo(selectedObject);
            }
            else
            {
                selectionText.text = "";
            }
        }

        currPlayerText.text = "Player " + currPlayer.ToString();
    }

    public void SetSelectedObject(GameObject s)
    {
        selectedObject = s;
    }

    public int SetCurrPlayer(int p)
    {
        currPlayer = p;

        return currPlayer;
    }

    public void SetCurrplayerText(string text)
    {
        currPlayerText.text = text;
    }

    void ShowUnitInfo(GameObject unit)
    {
        selectedObject = unit;

        KnightScript script = selectedObject.GetComponent<KnightScript>();

        selectionText.text = script.GetName() + "\n" + "Player: " + script.GetPlayer().ToString() + "\n"
            + "Movement: " + script.GetMovePoints().ToString();
    }

    void ShowTownInfo(GameObject town)
    {
        selectedObject = town;

        TownScript script = selectedObject.GetComponent<TownScript>();

        selectionText.text = "Town" + "\n" + "Player: " + script.GetPlayer().ToString() + "\n"
            + "Gold: " + script.GetGold();
    }

    void ShowCurrPlayer() {

    }

    public void OpenTownMenu()
    {
        townMenu.SetActive(true);
    }

    public void OrderBuildUnit()
    {
        selectedObject.GetComponent<TownScript>().BuildUnit();
    }

    public void CloseTownMenu()
    {
        townMenu.SetActive(false);
    }
}
