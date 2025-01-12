using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

[CreateAssetMenu(menuName = "Rendered Tile/GrimfieldTile", fileName = "GrimfieldTile")]
public class GrimfieldTile : Tile
{
    public bool isClaimed = false;
    public TileDefinition definition;
    public TileType Type
    {
        get => definition.type;
    }
    public int spriteIndex = 0;

    public void Init(TileDefinition definition)
    {
        this.definition = definition;
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
        if (!Application.isPlaying)
        {
            return;
        }
        try
        {
            if (Application.isPlaying)
            {
                tileData.sprite = definition.sprites[spriteIndex];
            }
            else tileData.sprite = definition.sprites[0];
        }
        catch (IndexOutOfRangeException)
        {
            //Debug.LogError($"Index was out of bounds, for: {definition} {position} index: {spriteIndex}");
        }

        if(definition.tilings.Count != 0)
        {
            Tilemap map = tilemap.GetComponent<Tilemap>();
            Direction8D direction = map.GetNeighbourTiling(this, position);

            foreach (NeighbourTiling tiling in definition.tilings)
            {
                if(tiling.Directions == direction)
                {
                    tileData.sprite = tiling.sprite;
                }
            }
        }

        name = definition.type.ToString();
    }
}
