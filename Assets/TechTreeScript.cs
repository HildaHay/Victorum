using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechTreeScript : MonoBehaviour
{
    Tech[] TechList;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GetTechList()
    {
        
    }
}

class Tech
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
