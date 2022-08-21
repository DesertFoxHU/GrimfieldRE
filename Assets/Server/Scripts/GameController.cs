using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ServerSide
{
    public class GameController : MonoBehaviour
    {
        [HideInInspector] public ChunkManager chunkManager;
        public int Seed = -1;
        public int SizeX;
        public int SizeY;
        public TurnHandler turnHandler;

        public void StartMatchGame()
        {
            turnHandler = new TurnHandler();

            NetworkManager.Instance.Lobby = null;
            NetworkManager.Instance.State = ServerState.Playing;
            ServerSender.SyncPlayers();
            GeneretaMap();

            Message startGamePacket = Message.Create(MessageSendMode.reliable, ServerToClientPacket.LoadGameScene);
            NetworkManager.Instance.Server.SendToAll(startGamePacket);
        }

        [ContextMenu("GenerateMap")]
        private void GeneretaMap()
        {
            if (Seed == -1) Seed = (int)System.DateTime.Now.Ticks;
            Random.InitState(Seed);

            Tilemap map = GameObject.FindGameObjectWithTag("GameMap").GetComponent<Tilemap>();

            chunkManager = new ChunkManager(SizeX, SizeY);
            for (int x = 0; x <= SizeX; x++)
            {
                for (int y = 0; y <= SizeY; y++)
                {
                    float perlin = Mathf.PerlinNoise((x + Seed / 100f) / 10f, (y + Seed / 100f) / 10f);
                    if (perlin <= 0.20f)
                    {
                        if (Utils.Roll(1.5f))
                        {
                            GenerateTile(map, x, y, TileType.DragonNest);
                            continue;
                        }

                        GenerateTile(map, x, y, TileType.Mountain);
                        continue;
                    }

                    if (perlin <= 0.25f && Utils.Roll(12f))
                    {
                        GenerateTile(map, x, y, TileType.GoldOre);
                        continue;
                    }

                    if (perlin <= 0.35f)
                    {
                        GenerateTile(map, x, y, TileType.Forest);
                        continue;
                    }

                    float perlin2 = Mathf.PerlinNoise((x + Seed / 100f) / 10f + 10f, (y + Seed / 100f) / 10f - 10f); //Offset by +10,-10
                    if (perlin2 <= 0.25f)
                    {
                        GenerateTile(map, x, y, TileType.ShallowWater);
                        continue;
                    }

                    GenerateTile(map, x, y, TileType.Grass);
                }
            }

            map.RefreshAllTiles();
            Debug.Log("Map is generated!");
        }

        [ContextMenu("Fill Building Resources")]
        private void InfiniteResources()
        {
            foreach (ServerPlayer player in NetworkManager.players)
            {
                foreach(ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
                    player.TryStoreResource(type, 9999);
            }
        }

        private void GenerateTile(Tilemap map, int x, int y, TileType type)
        {
            TileDefiniton definition = DefinitionRegistry.Instance.Find(type);
            int spriteIndex = definition.GetRandomSpriteIndex();
            map.SetTileSprite(new Vector3Int(x, y, 0), definition.sprites[spriteIndex]);
            chunkManager.SetTile(x, y, type, spriteIndex);
        }

        public void SendMapToAll()
        {
            chunkManager.chunks.ForEach(chunk => NetworkManager.Instance.Server.SendToAll(chunk.AsPacket(MessageSendMode.reliable, (ushort)ServerToClientPacket.ChunkInfo)));
        }

        public void SendMapTo(ushort clientID)
        {
            chunkManager.chunks.ForEach(chunk =>
            NetworkManager.Instance.Server.Send(chunk.AsPacket(
                MessageSendMode.reliable,
                (ushort)ServerToClientPacket.ChunkInfo), clientID));
        }
    }
}