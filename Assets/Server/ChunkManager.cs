using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager
{
    public int MapSizeX { get; private set; }
    public int MapSizeY { get; private set; }

    public List<Chunk> chunks = new List<Chunk>();

    public ChunkManager(int mapSizeX, int mapSizeY)
    {
        MapSizeX = mapSizeX;
        MapSizeY = mapSizeY;

        int ChunkX = Mathf.CeilToInt((float)MapSizeX / 4f);
        int ChunkY = Mathf.CeilToInt((float)MapSizeY / 4f);

        for (int x = 0; x < ChunkX; x++)
            for (int y = 0; y < ChunkY; y++)
                chunks.Add(new Chunk(x, y));
    }

    public void SetTile(int x, int y, TileType type)
    {
        //Can cause errors due being non-floating number
        Chunk chunk = GetChunkByPosition(x, y);
        Vector3Int v3 = new Vector3Int(x, y, 0);
        if (!chunk.Tiles.ContainsKey(v3))
        {
            chunk.Tiles.Add(v3, type);
        }
        else chunk.Tiles[v3] = type;
    }

    public Chunk GetChunkByPosition(int PosX, int PosY)
    {
        return chunks.Find(a => a.ChunkX == (PosX/4) && a.ChunkY == (PosY / 4));
    }

    public Chunk GetChunk(int ChunkX, int ChunkY)
    {
        return chunks.Find(a => a.ChunkX == ChunkX && a.ChunkY == ChunkY);
    }
}
