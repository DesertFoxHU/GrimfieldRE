using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public class TileData
    {
        public Vector3Int Pos;
        public TileType Type;
        public int SpriteIndex; //The sprite index in TileDefinition's sprites

        public TileData(Vector3Int pos)
        {
            Pos = pos;
        }
    }

    public readonly int ChunkX;
    public readonly int ChunkY;
    public List<TileData> Tiles = new();

    public Chunk(int chunkX, int chunkY)
    {
        ChunkX = chunkX;
        ChunkY = chunkY;
    }

    public TileData GetOrCreate(Vector3Int Pos)
    {
        TileData data = Tiles.Find(a => a.Pos == Pos);
        if (data == null)
        {
            data = new TileData(Pos);
            Tiles.Add(data);
        }
        return data;
    }

    public Message AsPacket(MessageSendMode mode, ushort packetId)
    {
        Message message = Message.Create(mode, packetId);
        message.Add(ChunkX);
        message.Add(ChunkY);
        message.Add(Tiles.Count);
        foreach(TileData data in Tiles)
        {
            message.Add(data.Pos.x + "|" + data.Pos.y + "|" + data.Type + "|" + data.SpriteIndex);
        }
        return message;
    }

}
