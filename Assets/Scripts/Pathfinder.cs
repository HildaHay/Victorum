using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pathfinder : MonoBehaviour
{
    public static List<Vector2Int> Path(Vector2Int start, Vector2Int destination) {
        List<Vector2Int> q = new List<Vector2Int>();

        Vector2Int mapDims = new Vector2Int(9, 9);

        bool[,] map = new bool[mapDims.x, mapDims.y];

        int[,] distanceVals = new int[mapDims.x, mapDims.y];

        Vector2Int[,] prevVertex = new Vector2Int[mapDims.x, mapDims.y];

        for (int i = 0; i < mapDims.x; i++)
        {
            for (int j = 0; j < mapDims.y; j++)
            {
                map[i, j] = true;
            }
        }

        for (int i = 0; i < mapDims.x; i++)
        {
            for (int j = 0; j < mapDims.y; j++)
            {
                distanceVals[i, j] = Int32.MaxValue;
                if (map[i, j])
                {
                    q.Add(new Vector2Int(i, j));
                }
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
                if (distanceVals[curr.x, curr.y] <= min)
                {
                    min = distanceVals[curr.x, curr.y];
                    minIndex = i;
                }
            }

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
                    prevVertex[minVertex.x, minVertex.y + 1] = new Vector2Int(minVertex.x, minVertex.y);
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
        }

        // All tiles' distances have been found
        // Now, find the shortest path

        Vector2Int backtrack = new Vector2Int(destination.x, destination.y);
        List<Vector2Int> path = new List<Vector2Int>();

        while(backtrack != start)
        {
            path.Insert(0, backtrack);
            backtrack = prevVertex[backtrack.x, backtrack.y];
        }
        path.Insert(0, backtrack);

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

        foreach(Vector2Int v in path)
        {
            Debug.Log(v.x + ", " + v.y);
        }

        return path;
    }

    //public static List<Vector2Int> Path(Vector2Int start, Vector2Int destination)
    //{

    //    return null;
    //}
}
