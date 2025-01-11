using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

internal class ChunkLoader
{
    public static void LoadChunk(Tilemap map, Message message)
    {
        Task task = new(() =>
        {
            int chunkX = message.GetInt();
            int chunkY = message.GetInt();
            int listCount = message.GetInt();

            for (int i = 0; i < listCount; i++)
            {
                string raw = message.GetString();
                string[] split = raw.Split('|');
                Vector3Int pos = new(int.Parse(split[0]), int.Parse(split[1]), 0);
                TileType tileType = (TileType)Enum.Parse(typeof(TileType), split[2]);
                int spriteIndex = int.Parse(split[3]);

                TileDefinition tileDef = DefinitionRegistry.Instance.Find(tileType);
                GrimfieldTile tile = map.GetOrInit(pos, tileDef);
                tile.spriteIndex = spriteIndex;

                map.RefreshTile(pos);
            }
        });
        task.Start();
    }

}
