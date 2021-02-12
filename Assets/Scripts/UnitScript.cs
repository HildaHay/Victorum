using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScript : MonoBehaviour
{
    // Start is called before the first frame update

    public int cost;

    public int maxMovement;
    protected int movementPoints;

    public int maxHP;
    public int HP;

    public int minDamage;
    public int maxDamage;

    public int armor;

    public int range;

    public int visionRange;

    protected int playerNumber;

    public string unitName = "";

    public int mapX;
    public int mapY;

    public List<Vector2Int> savedPath;

    public bool isTownBuilder;

    protected WorldManager worldManager;
    protected Player player;

    public GameObject playerIndicatorSprite;
    public List<GameObject> pathMarkers;

    public GameObject pathMarkerPrefab;

    public Sprite[] sprites;

    protected void Start()
    {
        // maxMovement = 3;
        movementPoints = maxMovement;

        HP = maxHP;

        pathMarkers = new List<GameObject>();
    }

    public void Initialize(WorldManager wm)
    {
        playerNumber = -1;
        worldManager = wm;
    }

    public void Initialize(Player p, WorldManager wm)
    {
        player = p;
        playerNumber = p.playerNumber;
        worldManager = wm;

        playerIndicatorSprite.GetComponent<SpriteRenderer>().sprite = sprites[p.playerNumber];
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Placeholder function; should call TryAttackUnit or TryAttackTown as appropriate
    // To be implemented
    public bool TryAttack()
    {
        return false;
    }

    public bool TryAttackUnit(UnitScript target)
    {

        if(this.playerNumber == target.playerNumber)
        {
            return false;
        }

        int distance = (Math.Abs(mapX - target.mapX) + Math.Abs(mapY - target.mapY));

        if (distance > range)
        {
            Debug.Log("Out of range");
            return false;
        }
        if (movementPoints <= 0)
        {
            Debug.Log("Out of movement.");
            return false;
        }

        movementPoints--;
        return true;
    }

    public bool TryAttackTown(TownScript target)
    {

        if (this.playerNumber == target.playerNumber)
        {
            return false;
        }

        int distance = (Math.Abs(mapX - target.mapX) + Math.Abs(mapY - target.mapY));

        if (distance > range)
        {
            Debug.Log("Out of range");
            return false;
        }
        if (movementPoints <= 0)
        {
            Debug.Log("Out of movement.");
            return false;
        }

        movementPoints--;
        return true;
    }

    public float AttackDamage()
    {
        float baseDamage = UnityEngine.Random.Range((float)minDamage - 0.49f, (float)maxDamage + 0.49f);
        // the 0.49 should make the damage outputs be randomly distributed over the range after being rounded
        // Otherwise, the distribution will be biased against the min and max values

        // return baseDamage * player.ShrineDamageBonus();
        return baseDamage * player.ShrineDamageBonus();
    }

    public bool ReceiveDamage(float d)
    {
        // float rawDamage = d / player.ShrineDefenseBonus() - armor;
        float rawDamage = 0;

        if (playerNumber == -1)
        {
            // Neutral units get no shrine bonus
            rawDamage = d - armor;
        }
        else
        {
            rawDamage = d / player.ShrineDefenseBonus() - armor;
        }

        print(rawDamage);

        int roundedDamage = Mathf.RoundToInt(rawDamage);


        HP -= System.Math.Max(roundedDamage, 0);

        if (HP <= 0)
        {
            Die();
            return true;
        }
        else
        {
            return false;
        }
    }

    protected void Die()
    {
        worldManager.DeleteUnit(this.gameObject);
    }

    public bool TryMove(int x)
    {
        if (movementPoints < x)
        {
            print("not enough movement points");
            return false;
        }
        else
        {
            movementPoints -= x;
            return true;
        }
    }
    
    // Wrapper for SelectDestination
    public int SelectDestination(int x, int y)
    {
        return SelectDestination(new Vector2Int(x, y), false);
    }

    // Function takes a tile coordinate and a bool as an argument
    // On the first click, this executes pathfinding and saves the path.
    // On the second click on the same tile, if the path is still the shortest valid path to that tile, move the unit; otherwise, find a new path.
    // If moveImmediately is set to true, the unit will move on the first click.
    // Returns 0 if no valid path found, 1 if a new path was found but the unit didn't move, and 2 if the unit successfully moved
    public int SelectDestination(Vector2Int destination, bool moveImmediately)
    {
        if (!worldManager.Walkable(destination.x, destination.y) || !player.tilesExplored[destination.x, destination.y])
        {
            return 0;
        }
        else
        {
            // find a path to the destination

            List<Vector2Int> newPath = CreateMovePath(destination);

            if(newPath == null)
            {
                // no valid path found
                return 0;
            }

            // Check if new path is the same as the saved path
            // Note: this also implicitly confirms that the saved path is the shortest valid path
            bool pathsMatch = true;
            if (newPath.Count == savedPath.Count)
            {
                for (int i = 0; i < newPath.Count; i++)
                {
                    if (newPath[i] != savedPath[i])
                    {
                        pathsMatch = false;
                    }
                }
            }
            else
            {
                pathsMatch = false;
            }

            savedPath = newPath;
            if (pathsMatch || moveImmediately)
            {
                Move();
                return 2;
            }
            else
            {
                return 1;
            }
        }
    }

    // Wrapper for SelectDestination that specifies to execute the move immediately
    // This should only ever return 0 or 2, never 1
    public int SelectDestinationAndMove(Vector2Int destination)
    {
        return SelectDestination(destination, true);
    }

    // Move the unit along the pre-created path
    public bool Move()
    {
        if (savedPath.Count > 0)
        {
            // Make sure that the path isn't blocked
            foreach (Vector2Int t in savedPath)
            {
                if (!worldManager.Walkable(t.x, t.y))
                {
                    // If the path is blocked, clear it - need to find a new one
                    Debug.Log("Path is blocked - can't go!");
                    ClearPath();
                    return false;
                }
            }

            while (savedPath.Count > 0 && movementPoints > 0)
            {
                Vector2Int nextTile = savedPath[0];
                savedPath.RemoveAt(0);
                movementPoints--;

                Vector2Int prevLocation = new Vector2Int(mapX, mapY);

                mapX = nextTile.x;
                mapY = nextTile.y;

                worldManager.unitGrid[prevLocation.x, prevLocation.y] = null;
                worldManager.unitGrid[mapX, mapY] = this.gameObject;

                // unit.transform.position = new Vector3(mapX, mapY, -1);
                this.gameObject.transform.position = worldManager.MapToScreenCoordinates(mapX, mapY, -1);

                if (playerNumber >= 0)    // neutral units don't track vision or capture shrines
                {
                    player.CheckVision(mapX, mapY, visionRange);
                    worldManager.CaptureShrine(nextTile, player.playerNumber);
                }
            }
            DrawPath();
            Camera.main.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, Camera.main.transform.position.z);
            return true;
        }
        else
        {
            return false;
        }
    }

    public Vector2Int GetSavedDestination()
    {
        if (savedPath.Count > 0)
        {
            return savedPath[savedPath.Count - 1];
        }
        else
        {
            return new Vector2Int(mapX, mapY);
        }
    }

    public List<Vector2Int> CreateMovePath(Vector2Int destination)
    {
        List<Vector2Int> newPath = Pathfinder.Path(new Vector2Int(mapX, mapY), destination, worldManager.WalkableMap());

        return newPath;
    }

    public void DrawPath()
    {
        // to do
        ClearPath();
        if (playerNumber >= 0)
        {
            foreach (Vector2Int a in savedPath)
            {
                pathMarkers.Add(Instantiate(pathMarkerPrefab, worldManager.MapToScreenCoordinates(a.x, a.y, -1), Quaternion.identity));
            }
        }
    }

    public void ClearPath()
    {
        pathMarkers.ForEach(Destroy);
        pathMarkers.Clear();
    }

    public string GetName()
    {
        return unitName;
    }

    public int GetPlayer()
    {
        return playerNumber;
    }

    public int SetPlayer(Player p)
    {
        player = p;

        playerNumber = p.playerNumber;
        return playerNumber;
    }

    public int GetMovePoints()
    {
        return movementPoints;
    }

    public void ResetMovePoints()
    {
        movementPoints = maxMovement;
    }

    // Used for AI management, for units that it can't/doesn't want to move
    // Should be replaced with an "inactive/ignore" tag
    public void ZeroMovePoints()
    {
        movementPoints = 0;
    }

    public Vector2Int xy()
    {
        return new Vector2Int(mapX, mapY);
    }

    public int MapDistance(Vector2Int start, Vector2Int end)
    {
        return Math.Abs(start[0] - end[0]) + Math.Abs(start[1] - end[1]);
    }
}
