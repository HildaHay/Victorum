using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechTreeScript : MonoBehaviour
{
    public Tech[] TechList;

    public Player player;

    [SerializeField] GameObject techMenuItemPrefab;
    GameObject techTreeUI;

    Tech techInProgress;

    GameObject[] TechMenuItems;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize()
    {
        CreateDemoTechs(null);
        CreateTechMenu();
        Debug.Log("Techs: " + TechList.Length);
    }

    void CreateDemoTechs(Player p)
    {
        Tech town = new Tech(0, "Town", true, 0, new int[0], new Tech[0], p);
        Tech builder = new Tech(1, "Builder", false, 10, new Tech[] { town }, p);
        Tech slinger = new Tech(2, "Slinger", false, 10, new Tech[] { town, builder }, p);

        TechList = new Tech[] { town, builder, slinger };
    }

    void CreateTechMenu()
    {
        int i = 0;

        if(player.IsHuman())
        {
            techTreeUI = GameObject.Find("TechTreeMenu");
            TechMenuItems = new GameObject[TechList.Length];

            foreach (Tech t in TechList)
            {
                GameObject newMenuItem = Instantiate(techMenuItemPrefab, techTreeUI.transform);
                newMenuItem.GetComponent<TechMenuItem>().tech = t;

                TechMenuItems[i] = newMenuItem;
                i++;
            }
        }

        int maxtier = 0;

        foreach(Tech t in TechList)
        {
            DetermineTechTier(t);
            if(t.Tier > maxtier)
            {
                maxtier = t.Tier;
            }
        }

        if (player.IsHuman())
        {
            // TODO: Add vertical spacing for techs in same tier
            foreach (GameObject o in TechMenuItems)
            {
                float horizPosition = 1000 * o.GetComponent<TechMenuItem>().tech.Tier / maxtier - 500;
                o.transform.position += new Vector3(horizPosition, 0, 0);
            }
        }
    }

    void DetermineTechTier(Tech t)
    {
        Tech[] prereqs = t.GetPrerequisites();
        if(prereqs.Length == 0)
        {
            t.Tier = 0;
        } else
        {
            t.Tier = 0;
            foreach(Tech p in prereqs)
            {
                if(p.Tier == -1)
                {
                    DetermineTechTier(p);
                }
                if(p.Tier > t.Tier)
                {
                    t.Tier = p.Tier;
                }
            }
            t.Tier += 1;
        }
    }

    public bool TechUnlocked(string techName)
    {
        foreach(Tech t in TechList)
        {
            if(techName.Equals(t.Name()))
            {
                return t.Unlocked();
            }
        }
        Debug.Log("Tech not found!");
        return false;
    }

    // Debug function - remove later
    public Tech NextLockedTech() {
        foreach(Tech t in TechList)
        {
            if(!t.Unlocked())
            {
                return t;
            }
        }
        return null;
    }

    public void ProgressResearch(int r)
    {
        if (techInProgress == null)
            techInProgress = NextLockedTech();

        techInProgress.AdvanceResearch(r);  // todo: remaining progress should be saved
        if(techInProgress.Unlocked())
        {
            techInProgress = null;
        }

        if (techInProgress == null)
            techInProgress = NextLockedTech();
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

    public int Tier;    // Move this later?

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

        Tier = -1;

        Progress = 0;
    }

    public Tech(int ID, string n, bool u, int c, Tech[] pre, Player pl)
    {
        TechID = ID;
        TechName = n;
        IsUnlocked = u;
        TechCost = c;
        Prerequisites = pre;
        Player = pl;

        PrereqIDs = new int[pre.Length];
        for(int i = 0; i < pre.Length; i++)
        {
            PrereqIDs[i] = pre[i].ID();
        }

        Tier = -1;

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
        Debug.Log(r);
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
                Debug.Log("Tech completed: " + TechName);
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
