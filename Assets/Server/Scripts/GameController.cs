using Riptide;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ServerSide
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance;

        private void Start()
        {
            Instance = this;
        }

        [HideInInspector] public ChunkManager chunkManager;
        public int Seed = -1;
        [Range(1, 128)] public int SizeX;
        [Range(1, 128)] public int SizeY;
        public TurnHandler turnHandler;

        private Tilemap map;

        public void StartMatchGame()
        {
            map = GameObject.FindGameObjectWithTag("GameMap").GetComponent<Tilemap>();
            turnHandler = new TurnHandler();

            NetworkManager.Instance.Lobby = null;
            NetworkManager.Instance.State = ServerState.Playing;
            ServerSender.SyncPlayers();
            GeneretaMap();

            Message startGamePacket = Message.Create(MessageSendMode.reliable, ServerToClientPacket.LoadGameScene);
            NetworkManager.Instance.Server.SendToAll(startGamePacket);
        }

        public Entity SpawnUnit(ServerPlayer? player, Vector3Int position, EntityType type)
        {
            EntityDefinition definition = FindAnyObjectByType<DefinitionRegistry>().Entities.Find(x => x.Type == type);
            if (definition == null)
            {
                Debug.LogError($"Can't find any EntityDefinition with this type: {type}");
                return null;
            }

            Vector3 v3 = map.ToVector3(position);
            GameObject go = Instantiate(definition.Prefab, new Vector3(v3.x + 0.5f, v3.y + 0.5f, -1.1f), Quaternion.identity);

            Entity entity = go.GetComponent<Entity>();
            entity.Initialize(position, definition, EntityManager.GetNextEntityId());
            if (player != null)
            {
                entity.SetOwner(player.PlayerId);
                entity.SetColor(player.Color);
            }

            string playerId = player == null ?  "" : "" + player.PlayerId;

            Message newMessage = Message.Create(MessageSendMode.reliable, ServerToClientPacket.SpawnEntity);
            newMessage.Add(playerId);
            newMessage.Add(type.ToString());
            newMessage.Add(position);
            newMessage.Add(entity.Id);
            NetworkManager.Instance.Server.SendToAll(newMessage);
            return entity;
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
            ServerSide.TerritoryRenderer.Instance.RenderAll();
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
            TileDefinition definition = DefinitionRegistry.Instance.Find(type);
            int spriteIndex = definition.GetRandomSpriteIndex();
            Debug.Log($"Generated spriteIndex {spriteIndex} {x} {y} for {type}");

            GrimfieldTile tile = map.GetOrInit(new Vector3Int(x, y, 0), definition);
            tile.spriteIndex = spriteIndex;
            chunkManager.SetTile(x, y, type, spriteIndex);

            map.RefreshTile(new Vector3Int(x, y, 0));
        }

        public void SendMapToAll()
        {
            NetworkManager.players.ForEach(p =>
            {
                SendMapAsync(p.PlayerId);
            });
        }

        public async void SendMapAsync(ushort clientID)
        {
            ServerSender.InitChunkLoadingState(NetworkManager.Find(clientID), chunkManager.chunks.Count);

            int delay = 20;
            foreach (var chunk in chunkManager.chunks)
            {
                NetworkManager.Instance.Server.Send(chunk.AsPacket(
                    MessageSendMode.reliable,
                    (ushort)ServerToClientPacket.LoadChunk
                    ), clientID);
                await Task.Delay(delay);
            }
        }
    }
}