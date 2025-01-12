using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Universal;

public class TerritoryRendererDebug : MonoBehaviour
{
    public Color color;
    public int range;

    private void Awake()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 worldPoint = ray.GetPoint(-ray.origin.z / ray.direction.z);
            Vector3Int pos = GameObject.FindGameObjectWithTag("GameMap").GetComponent<Tilemap>().ToVector3Int(worldPoint);

            List<Vector3Int> vectors = GameObject.FindGameObjectWithTag("GameMap").GetComponent<Tilemap>().GetTileRange(pos, range);
            FindAnyObjectByType<TerritoryRenderer>().Render(vectors, color);
        }
    }
}
