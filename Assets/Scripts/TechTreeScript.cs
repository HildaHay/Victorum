using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechTreeScript : MonoBehaviour
{
    public Tech[] TechList;

    [SerializeField] GameObject techMenuItemPrefab;

    [SerializeField] GameObject techTreeUI;

    // Start is called before the first frame update
    void Start()
    {
        CreateDemoTechs(null);
        CreateTechMenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GetTechList()
    {
        
    }

    void CreateDemoTechs(Player p)
    {
        Tech town = new Tech(0, "Town", true, 0, new int[0], new Tech[0], p);
        Tech builder = new Tech(1, "Builder", true, 10, new int[] { 0 }, new Tech[] { town }, p);
        Tech slinger = new Tech(2, "Slinger", true, 10, new int[] { 0 }, new Tech[] { town }, p);

        TechList = new Tech[] { town, builder, slinger };
    }

    void CreateTechMenu()
    {
        foreach(Tech t in TechList)
        {
            GameObject newMenuItem = Instantiate(techMenuItemPrefab, techTreeUI.transform);
            newMenuItem.GetComponent<TechMenuItem>().tech = t;
        }
    }
}

public class Tech
{
    // Tech info, stored in file
    int TechID;
    string TechName;
    int TechCost;
    int[] PrereqIDs;
    string TechDescription;

    // Links to the tech's prerequisite objects
    Tech[] Prerequisites;

    // Player who this tech object belongs to
    Player Player;

    int Progress;
    bool IsUnlocked;

    public Tech(int ID, string n, bool u, int c, int[] pID, Tech[] pre, Player pl)
    {
        TechID = ID;
        TechName = n;
        IsUnlocked = u;
        TechCost = c;
        PrereqIDs = pID;
        Prerequisites = pre;
        Player = pl;

        Progress = 0;
    }

    public int ID()
    {
        return TechID;
    }

    public string Name()
    {
        return TechName;
    }

    public bool Unlocked()
    {
        return IsUnlocked;
    }

    public int Cost()
    {
        return TechCost;
    }

    // Advances the tech's research by the given amount and checks if research on it is completed
    // If the tech is completed, mark it unlocked and return the excess research points (to be used on next tech)
    public int AdvanceResearch(int r)
    {
        if(IsUnlocked)
        {
            return r;
        } else
        {
            Progress += r;
            if (Progress >= TechCost)
            {
                IsUnlocked = true;
                int leftover = Progress - TechCost;
                Progress = TechCost;
                return leftover;
            } else
            {
                return 0;
            }
        }
    }

    public Tech[] GetPrerequisites()
    {
        return Prerequisites;
    }

    public bool PrerequisitesCompelete()
    {
        foreach(Tech t in Prerequisites)
        {
            if(!t.Unlocked())
            {
                return false;
            }
        }
        return true;
    }
}
