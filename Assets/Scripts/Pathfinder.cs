using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pathfinder : MonoBehaviour
{
    // This is a function intended for use when looking at only a subsection of the map
    // offset should be the coordinate of the top-left corner of the subsection
    // This function should be replaced with a more robust one for this purpose
    public static List<Vector2Int> OffsetPath(Vector2Int start, Vector2Int destination, bool[,] map, Vector2Int offset)
    {
        return Path(start - offset, destination - offset, map);
    }

    public static List<Vector2Int> Path(Vector2Int start, Vector2Int destination, bool[,] map)
    {
        List<Vector2Int> q = new List<Vector2Int>();

        Vector2Int mapDims = new Vector2Int(map.GetLength(0), map.GetLength(1));

        // Make sure the starting and ending positions are actually within the map area
        if(start.x < 0 || start.y < 0 || destination.x < 0 || destination.y < 0
            || start.x >= mapDims.x || start.y >= mapDims.y || destination.x >= mapDims.x || destination.y >= mapDims.y)
        {
            Debug.Log("Pathing: start or destination was outside of map");
            return null;
        }

        int[,] distanceVals = new int[mapDims.x, mapDims.y];

        Vector2Int[,] prevVertex = new Vector2Int[mapDims.x, mapDims.y];

        for (int i = 0; i < mapDims.x; i++)
        {
            for (int j = 0; j < mapDims.y; j++)
            {
                distanceVals[i, j] = Int32.MaxValue;
                q.Add(new Vector2Int(i, j));
            }
        }

        distanceVals[start.x, start.y] = 0;
        prevVertex[start.x, start.y] = new Vector2Int(start.x, start.y);

        while (q.Count > 0)
        {
            // find one with min distance
            int min = Int32.MaxValue;
            int minIndex = -1;

            for (int i = 0; i < q.Count; i++)
            {
                Vector2Int curr = q[i];
                if (distanceVals[curr.x, curr.y] < min)
                {
                    min = distanceVals[curr.x, curr.y];
                    minIndex = i;
                }
            }

            // If minIndex is still -1, all remaining tiles' distance values are Int32.MaxValue. That means none of them are accessible
            // from the starting tile, so all accessible tiles have already been explored and we can go on to the next step
            if (minIndex >= 0)
            {
                Vector2Int minVertex = q[minIndex];
                q.RemoveAt(minIndex);

                // set neighbours' distance
                if (minVertex.x > 0)
                {
                    if (map[minVertex.x - 1, minVertex.y]
                        && distanceVals[minVertex.x - 1, minVertex.y] > distanceVals[minVertex.x, minVertex.y] + 1)
                    {
                        distanceVals[minVertex.x - 1, minVertex.y] = distanceVals[minVertex.x, minVertex.y] + 1;
                        prevVertex[minVertex.x - 1, minVertex.y] = new Vector2Int(minVertex.x, minVertex.y);
                    }
                }
                if (minVertex.x < mapDims.x - 1)
                {
                    if (map[minVertex.x + 1, minVertex.y]
                        && distanceVals[minVertex.x + 1, minVertex.y] > distanceVals[minVertex.x, minVertex.y] + 1)
                    {
                        distanceVals[minVertex.x + 1, minVertex.y] = distanceVals[minVertex.x, minVertex.y] + 1;
                        prevVertex[minVertex.x + 1, minVertex.y] = new Vector2Int(minVertex.x, minVertex.y);
                    }
                }
                if (minVertex.y > 0)
                {
                    if (map[minVertex.x, minVertex.y - 1]
                        && distanceVals[minVertex.x, minVertex.y - 1] > distanceVals[minVertex.x, minVertex.y] + 1)
                    {
                        distanceVals[minVertex.x, minVertex.y - 1] = distanceVals[minVertex.x, minVertex.y] + 1;
                        prevVertex[minVertex.x, minVertex.y - 1] = new Vector2Int(minVertex.x, minVertex.y);
                    }
                }
                if (minVertex.y < mapDims.y - 1)
                {
                    if (map[minVertex.x, minVertex.y + 1]
                        && distanceVals[minVertex.x, minVertex.y + 1] > distanceVals[minVertex.x, minVertex.y] + 1)
                    {
                        distanceVals[minVertex.x, minVertex.y + 1] = distanceVals[minVertex.x, minVertex.y] + 1;
                        prevVertex[minVertex.x, minVertex.y + 1] = new Vector2Int(minVertex.x, minVertex.y);
                    }
                }
            } else
            {
                q.Clear();
            }
        }

        if (distanceVals[destination.x, destination.y] == Int32.MaxValue)
        {
            Debug.Log("No path found.");
            return null;
        }

        // All tiles' distances have been found
        // Now, find the shortest path

        Vector2Int backtrack = new Vector2Int(destination.x, destination.y);
        List<Vector2Int> path = new List<Vector2Int>();

        while (backtrack != start)
        {
            path.Insert(0, backtrack);
            backtrack = prevVertex[backtrack.x, backtrack.y];
        }
        // path.Insert(0, backtrack);

        /* String toPrint = "";
        for (int i = 0; i < mapDims.x; i++)
        {
            for (int j = 0; j < mapDims.y; j++)
            {
                toPrint += " ";
                if (distanceVals[i, j] < 10)
                {
                    toPrint += " ";
                }
                toPrint += distanceVals[i, j];
            }
            toPrint += "\n";
        }

        Debug.Log(toPrint); */

        return path;
    }

    //public static List<Vector2Int> Path(Vector2Int start, Vector2Int destination)
    //{

    //    return null;
    //}
}
