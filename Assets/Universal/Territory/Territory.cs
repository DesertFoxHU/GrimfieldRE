using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Territory
{
    public static AbstractBuilding[,] territoryGrid;

    public static void Start(int gridSizeX, int gridSizeY)
    {
        territoryGrid = new AbstractBuilding[gridSizeX, gridSizeY];
    }

    public static void Claim(int ClientId, AbstractBuilding building, List<Vector3Int> land)
    {
        foreach (var pos in land)
        {
            territoryGrid[pos.x, pos.y] = building;
        }
    }

    public static List<int> GetUniqueClients()
    {
        List<int> clients = new();
        foreach (var c in territoryGrid)
        {
            if (c != null)
            {
                if (!clients.Contains(c.OwnerId))
                {
                    clients.Add(c.OwnerId);
                }
            }
        }
        return clients;
    }

    public static List<AbstractBuilding> GetTerritoryNeighbors(int x, int y)
    {
        List<AbstractBuilding> neighbors = new();

        int[,] directions = new int[,]
        {
            { 0, 1 },  
            { 0, -1 }, 
            { 1, 0 }, 
            { -1, 0 },
        };
        AbstractBuilding origin = territoryGrid[x, y];

        int gridSizeX = territoryGrid.GetLength(0);
        int gridSizeY = territoryGrid.GetLength(1);

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int neighborX = x + directions[i, 0];
            int neighborY = y + directions[i, 1];

            if (neighborX >= 0 && neighborX < gridSizeX && neighborY >= 0 && neighborY < gridSizeY)
            {
                AbstractBuilding neighbor = territoryGrid[neighborX, neighborY];

                if (neighbor != null)
                {
                    if(origin != null)
                    {
                        if(origin.OwnerId != neighbor.OwnerId)
                        {
                            continue;
                        }
                    }
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }

    public static int GetTerritoryNeighborCount(int x, int y)
    {
        int count = 0;

        int[,] directions = new int[,]
        {
            { 0, 1 },
            { 0, -1 },
            { 1, 0 },
            { -1, 0 },
        };
        AbstractBuilding origin = territoryGrid[x, y];

        int gridSizeX = territoryGrid.GetLength(0);
        int gridSizeY = territoryGrid.GetLength(1);

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int neighborX = x + directions[i, 0];
            int neighborY = y + directions[i, 1];

            if (neighborX >= 0 && neighborX < gridSizeX && neighborY >= 0 && neighborY < gridSizeY)
            {
                AbstractBuilding neighbor = territoryGrid[neighborX, neighborY];

                if (neighbor != null)
                {
                    if (origin != null)
                    {
                        if (origin.OwnerId != neighbor.OwnerId)
                        {
                            continue;
                        }
                    }
                    count++;
                }
            }
        }

        return count;
    }
}
