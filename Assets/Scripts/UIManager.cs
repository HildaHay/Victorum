using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject selectionTextObject;
    Text selectionText;

    public GameObject currPlayerTextObject;
    Text currPlayerText;

    public GameObject winnerTextObject;
    Text winnerText;

    public GameObject endTurnButton;
    public GameObject buildTownButton;
    public GameObject nextUnitButton;

    public GameObject recruitButtonPrefab;  

    Player currPlayer;

    GameObject selectedObject;

    public GameObject townMenu;
    List<GameObject> recruitButtons;
    GameObject[] availableUnits;

    // Start is called before the first frame update
    void Start()
    {
        selectionText = selectionTextObject.GetComponent<Text>();
        selectionText.text = "";

        currPlayerText = currPlayerTextObject.GetComponent<Text>();
        currPlayerText.text = "";

        winnerText = winnerTextObject.GetComponent<Text>();
        winnerText.text = "";

        townMenu.SetActive(false);

        recruitButtons = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {

        if(selectedObject == null)
        {
            buildTownButton.SetActive(false);
            selectionText.text = "";
        } else {
            if (selectedObject.tag == "Unit")
            {
                ShowUnitInfo(selectedObject);
                if (selectedObject.GetComponent<UnitScript>().isTownBuilder)
                {
                    buildTownButton.SetActive(true);
                } else
                {
                    buildTownButton.SetActive(false);
                }
            }
            else
            {
                buildTownButton.SetActive(false);

                if (selectedObject.tag == "Town")
                {
                    ShowTownInfo(selectedObject);
                }
                else if (selectedObject.tag == "MapFeature")
                {
                    ShowFeatureInfo(selectedObject);
                }
                else if (selectedObject.tag == "MapObjective")
                {
                    ShowObjectiveInfo(selectedObject);
                }
                else
                {
                    selectionText.text = "";
                }
            }

            if(selectedObject.tag != "Town")
            {
                CloseTownMenu();
            }
        }

        if(currPlayer != null) {
            currPlayerText.text = "Player " + currPlayer.playerNumber.ToString() + "\n"
                + "Gold: " + currPlayer.gold + " (+" + currPlayer.goldPerTurn() + " per turn)" + "\n"
                + "Shrines: " + currPlayer.ShrineCount();
        } else
        {
            currPlayerText.text = "";
        }
    }

    public void SetSelectedObject(GameObject s)
    {
        selectedObject = s;
    }

    public Player SetCurrPlayer(Player p)
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

        UnitScript script = selectedObject.GetComponent<UnitScript>();

        selectionText.text = script.GetName() + "\n" + "Player " + script.GetPlayer().ToString() + "\n"
            + "Movement: " + script.GetMovePoints().ToString() + "\n"
            + "Health:" + script.HP.ToString();
    }

    void ShowTownInfo(GameObject town)
    {
        selectedObject = town;

        TownScript script = selectedObject.GetComponent<TownScript>();

        selectionText.text = "Town" + "\n" + "Player: " + script.GetPlayer().ToString() + "\n"
            + "Gold: " + script.GetGold();
    }

    void ShowFeatureInfo(GameObject feature)
    {
        selectedObject = feature;

        MapFeatureScript script = selectedObject.GetComponent<MapFeatureScript>();

        selectionText.text = script.featureName;
    }

    void ShowObjectiveInfo(GameObject objective)
    {
        selectedObject = objective;

        MapObjectiveScript script = selectedObject.GetComponent<MapObjectiveScript>();

        if (script.player == -1)
        {
            selectionText.text = script.objectiveName + "\n"
                + "Unclaimed";
        }
        else
        {
            selectionText.text = script.objectiveName + "\n"
                + "Player " + script.player;
        }
    }

    void ShowCurrPlayer() {

    }

    public void OpenTownMenu(GameObject[] townAvailableUnits)
    {
        availableUnits = townAvailableUnits;

        townMenu.SetActive(true);
        for (int i = 0; i < availableUnits.Length; i++)
        {
            GameObject newButton = Instantiate(recruitButtonPrefab, new Vector3(Screen.width / 2, Screen.height / 2, 0), Quaternion.identity, townMenu.transform);

            newButton.transform.position += new Vector3(0, (-35 * (availableUnits.Length - 1)) + (70 * i));
            newButton.GetComponentInChildren<Text>().text = availableUnits[i].GetComponent<UnitScript>().name;

            int unitIndex = i;
            newButton.GetComponent<Button>().onClick.AddListener(() => { OrderBuildUnit(unitIndex); });

            recruitButtons.Add(newButton);
        }

    }

    public void OrderBuildUnit(int unitIndex)
    {
        GameObject unitToBuild = availableUnits[unitIndex];
        selectedObject.GetComponent<TownScript>().OrderBuildUnit(unitToBuild);
    }

    public void CloseTownMenu()
    {
        foreach(GameObject b in recruitButtons)
        {
            Destroy(b);
        }
        recruitButtons.Clear();
        availableUnits = null;

        townMenu.SetActive(false);
    }

    public void SetWinner(int p)
    {
        winnerText.text = "Player " + p.ToString() + " wins!";
        selectedObject = null;
        currPlayer = null;

        selectionTextObject.SetActive(false);
        currPlayerTextObject.SetActive(false);
        endTurnButton.SetActive(false);

    }
}
