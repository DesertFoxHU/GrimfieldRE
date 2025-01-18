using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryRenderer : MonoBehaviour
{
    public void RenderAll()
    {
        foreach (Transform children in this.transform)
        {
            GameObject.Destroy(children.gameObject);
        }

        foreach (int clientId in Territory.GetUniqueClients())
        {
            RenderFor(clientId);
        }
    }

    private void RenderFor(int clientId)
    {
        Color color = Color.black;
        if (NetworkChecker.IsClient()) color = ClientSide.NetworkManager.Find(clientId).Color;
        else color = ServerSide.NetworkManager.Find(clientId).Color;

        List<Vector3> positions = new();
        List<AbstractBuilding> buildings = new();
        if (NetworkChecker.IsClient()) buildings = ClientSide.NetworkManager.Find(clientId).Buildings;
        else buildings = ServerSide.NetworkManager.Find(clientId).Buildings;
        foreach (AbstractBuilding building in buildings)
        {
            foreach(Vector3Int land in building.ClaimedLand)
            {
                int x = land.x;
                int y = land.y;

                if (Territory.GetTerritoryNeighborCount(x, y) < 4)
                {
                    positions.Add(new Vector3(land.x + 0.5f, land.y + 0.5f, -0.1f));
                }
            }
        }

        positions = SortPointsForLineRenderer(positions);

        GameObject go = new GameObject("territory_" + clientId + "_" + Time.realtimeSinceStartup);
        go.transform.parent = transform;
        go.transform.position = new Vector3(0, 0, 0);
        LineRenderer lineRenderer = go.AddComponent<LineRenderer>();
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
        lineRenderer.widthMultiplier = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.loop = true;
    }

    private List<Vector3> SortPointsForLineRenderer(List<Vector3> points)
    {
        if (points == null || points.Count == 0)
            return new List<Vector3>();

        List<Vector3> sorted = new List<Vector3>();
        HashSet<int> visited = new HashSet<int>();
        Vector3 current = points[0];
        sorted.Add(current);
        visited.Add(0);

        while (sorted.Count < points.Count)
        {
            float closestDistance = float.MaxValue;
            int closestIndex = -1;

            for (int i = 0; i < points.Count; i++)
            {
                if (visited.Contains(i)) continue;

                float distance = Vector3.Distance(current, points[i]);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }

            if (closestIndex != -1)
            {
                current = points[closestIndex];
                sorted.Add(current);
                visited.Add(closestIndex);
            }
        }

        return sorted;
    }

}
