using Riptide;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

namespace ServerSide
{
    [RequireComponent(typeof(GameController))]
    public class PacketHandler : MonoBehaviour
    {
        public static Tilemap map;
        private static GameController gameController;

        public void Start()
        {
            PacketHandler.map = GameObject.FindGameObjectWithTag("GameMap").GetComponent<Tilemap>();
            gameController = this.GetComponent<GameController>();
        }

        [MessageHandler((ushort)ClientToServerPacket.JoinLobby)]
        private static void JoinLobby(ushort clientID, Message message)
        {
            string name = message.GetString().Trim();

            if(name == null || name.Length == 0)
            {
                NetworkManager.Instance.Server.Send(
                    Message.Create(MessageSendMode.reliable,
                    ServerToClientPacket.SendAlert).AddString("Server: Your name is empty"),
                    clientID);
                NetworkManager.Instance.Server.DisconnectClient(clientID);
                return;
            }

            if (name.Length > 16)
            {
                NetworkManager.Instance.Server.Send(
                    Message.Create(MessageSendMode.reliable,
                    ServerToClientPacket.SendAlert).AddString("Server: Your name cannot be longer than 16 characters!"),
                    clientID);
                NetworkManager.Instance.Server.DisconnectClient(clientID);
                return;
            }

            Debug.Log($"{name} joined to the lobby!");
            new ServerPlayer(clientID, name, NetworkManager.Instance.Lobby.GetAnAvaibleColor());

            Message lobbyMsg = Message.Create(MessageSendMode.reliable, ServerToClientPacket.LoadLobby);
            FindObjectOfType<NetworkManager>().Server.Send(lobbyMsg, clientID);
        }

        [MessageHandler((ushort)ClientToServerPacket.ChangeReadyStatus)]
        private static void ChangeReadyStatus(ushort clientID, Message message)
        {
            ServerPlayer player = NetworkManager.Find(clientID);
            if(player == null)
            {
                Debug.LogError($"Unathorized ready status change from: {clientID}");
                return;
            }

            player.IsReady = message.GetBool();

            bool isEveryoneRead = true;
            foreach(ServerPlayer serverPlayer in NetworkManager.players)
            {
                if (!serverPlayer.IsReady)
                {
                    isEveryoneRead = false;
                    break;
                }
            }

            if (isEveryoneRead)
            {
                FindAnyObjectByType<GameController>().StartMatchGame();
            }
        }

        [MessageHandler((ushort)ClientToServerPacket.MainGameLoaded)]
        private static void OnMainGameLoaded(ushort clientID, Message message)
        {
            ServerPlayer player = NetworkManager.Find(clientID);
            player.IsMainSceneLoaded = true;
            gameController.SendMapAsync(clientID);

            bool everyoneReady = true;
            foreach (ServerPlayer sPlayer in NetworkManager.players)
            {
                if (!sPlayer.IsMainSceneLoaded)
                {
                    everyoneReady = false;
                    break;
                }
            }

            if(everyoneReady) ServerSender.TurnChange(gameController.turnHandler.turnOrder[gameController.turnHandler.currentIndex], gameController.turnHandler.turnCycleCount);
        }

        [MessageHandler((ushort)ClientToServerPacket.RequestBuild)]
        private static void RequestToBuild(ushort clientID, Message message)
        {
            if(clientID != gameController.turnHandler.GetCurrentTurnOwnerID())
            {
                ServerSender.SendAlert(clientID, "It's not your turn!");
                return;
            }

            Vector3 v3Pos = message.GetVector3();
            Vector3Int pos = GameObject.FindGameObjectWithTag("GameMap").GetComponent<Tilemap>().ToVector3Int(v3Pos);
            BuildingType type = (BuildingType) Enum.Parse(typeof(BuildingType), message.GetString());

            TileDefinition definition = map.GetTile<GrimfieldTile>(pos).definition;
            if (definition == null)
            {
                Debug.LogError("Definition for this position wasn't loaded!");
                return;
            }

            BuildingDefinition buildingDefinition = DefinitionRegistry.Instance.Find(type);
            if (!buildingDefinition.placeable.Contains(definition.type))
            {
                NetworkManager.Instance.Server.Send(
                    Message.Create(MessageSendMode.unreliable, ServerToClientPacket.SendAlert).Add("You can't place this building here"),
                    clientID);
                return;
            }

            bool occupied = !NetworkManager.GetAllBuilding().TrueForAll(x => x.Position != pos);
            if (occupied)
            {
                ServerSender.SendAlert(clientID, "This tile is already occupied!");
                return;
            }

            ServerPlayer player = NetworkManager.Find(clientID);

            Dictionary<ResourceType, double> cost = buildingDefinition.GetBuildingCost(player.BuildingBought.ContainsKey(type) ? player.BuildingBought[type] : 0);
            List<ResourceHolder> resources = player.GetAvaibleResources();

            if (cost.Count != 0 && !player.PayResources(cost))
            {
                ServerSender.SendAlert(clientID, "You don't have enough resources!");
                return;
            }

            AbstractBuilding building = (AbstractBuilding) Activator.CreateInstance(AbstractBuilding.GetClass(type), player, pos);

            player.Buildings.Add(building);
            player.IncrementBuildingBought(type);
            ServerSender.SendNewBuilding(player, building);

            FindAnyObjectByType<ServerSide.TerritoryRenderer>().TryAddNew(map, player, building);

            Debug.Log($"Added new building with GUID: {building.ID}");
        }

        [MessageHandler((ushort)ClientToServerPacket.NextTurn)]
        private static void NextTurn(ushort clientID, Message message)
        {
            if(gameController == null)
            {
                Debug.LogError("GameController is not initialized!");
                return;
            }

            if(gameController.turnHandler == null)
            {
                Debug.LogWarning("TurnHandler is not initialized!");
                return;
            }

            if(gameController.turnHandler.GetCurrentTurnOwnerID() != clientID)
            {
                NetworkManager.Instance.Server.Send(
                    Message.Create(MessageSendMode.unreliable, ServerToClientPacket.SendAlert).Add("Stop trolling! Its not your turn yet!"),
                    clientID);
                return;
            }

            gameController.turnHandler.TurnEnded();
        }

        [MessageHandler((ushort)ClientToServerPacket.FetchBuildingData)]
        private static void FetchBuildingData(ushort clientID, Message message)
        {
            Guid guid = Guid.Parse(message.GetString());
            MouseClickType clickType = (MouseClickType)Enum.Parse(typeof(MouseClickType), message.GetString());

            AbstractBuilding building = NetworkManager.GetAllBuilding().Find(x => x.ID == guid);
            if(building == null)
            {
                ServerSender.SendAlert(clientID, "Sync error: No building in this position");
                return;
            }

            Message response = Message.Create(MessageSendMode.unreliable, ServerToClientPacket.FetchBuildingDataResponse);
            response.Add(building);
            NetworkManager.Instance.Server.Send(response, clientID);
        }

        [MessageHandler((ushort)ClientToServerPacket.BuyEntity)]
        private static void BuyEntity(ushort clientID, Message message)
        {
            ServerPlayer player = NetworkManager.Find(clientID);
            if(GameController.Instance.turnHandler.GetCurrentTurnOwnerID() != clientID)
            {
                ServerSender.SendAlert(clientID, "You can only recruit when its your turn");
                return;
            }

            EntityType type = (EntityType) Enum.Parse(typeof(EntityType), message.GetString());
            Vector3Int position = message.GetVector3Int();

            AbstractBuilding building = player.Buildings.Find(x => x.Position == position);
            if (building == null)
            {
                Debug.LogWarning($"Player {player.Name} tried to recruit an unit in {position}, however the position is not a building. Probably delay error or cheat attempt.");
                return;
            }

            EntityDefinition definition = FindObjectOfType<DefinitionRegistry>().Find(type);
            if (!player.PayResources(definition.GetRecruitCost()))
            {
                ServerSender.SendAlert(clientID, $"Don't have enough resources to recruit this unit!");
                return;
            }

            Entity entity = GameController.Instance.SpawnUnit(player, position, type);
            if (entity != null) player.entities.Add(entity);
        }

        [MessageHandler((ushort)ClientToServerPacket.MoveEntity)]
        private static void MoveEntity(ushort clientID, Message message)
        {
            if(GameController.Instance.turnHandler.GetCurrentTurnOwnerID() != clientID)
            {
                ServerSender.SendAlert(clientID, "It's not your turn!");
                return;
            }

            int entityId = message.GetInt();
            Vector3Int from = message.GetVector3Int();
            Vector3Int to = message.GetVector3Int();

            Entity entity = FindObjectsOfType<Entity>().First(x => x.Id == entityId);
            if(entity == null)
            {
                ServerSender.SendAlert(clientID, "Didn't find any entity on that position. Can this be desync related error?");
                return;
            }

            if(entity.OwnerId != clientID)
            {
                ServerSender.SendAlert(clientID, "You are not owner of this unit!");
                return;
            }

            if(entity.Position.x != from.x || entity.Position.y != from.y)
            {
                Debug.LogError($"Desync error happened with entity: {entityId}. Client's position for this entity is {from} meanwhile serverside is {entity.Position}");
                Debug.Log("Trying to solve client's desync error...");
                Message responseFix = Message.Create(MessageSendMode.reliable, ServerToClientPacket.MoveEntity);
                responseFix.Add(entity.Id);
                responseFix.Add(from);
                responseFix.Add(entity.Position);
                responseFix.Add(entity.lastTurnWhenMoved);
                NetworkManager.Instance.Server.Send(message, clientID);
                ServerSender.SendAlert(clientID, "Unit desync error happened, tried to fix the problem");
                Debug.Log("Correction sent!");
                return;
            }

            WeightGraph graph = new WeightGraph(entity);
            if (!graph.GetMovementRange(entity).Contains(to))
            {
                ServerSender.SendAlert(clientID, "This unit can't move here.");
                return;
            }

            if (!EntityManager.OnMoveTo(NetworkManager.Find(clientID), map, entity, to)) return;

            Vector3 v3 = map.ToVector3(to);
            entity.gameObject.transform.position = new Vector3(v3.x + 0.5f, v3.y + 0.5f, -1.1f);
            entity.Position = to;
            entity.canMove = false;
            entity.OnMoved(from, to);

            Message response = Message.Create(MessageSendMode.reliable, ServerToClientPacket.MoveEntity);
            response.Add(entity.Id);
            response.Add(from);
            response.Add(to);
            response.Add(entity.lastTurnWhenMoved);
            NetworkManager.Instance.Server.SendToAll(response);
        }

        [MessageHandler((ushort)ClientToServerPacket.SendMessage)]
        private static void RecieveMessage(ushort clientID, Message message)
        {
            string text = message.GetString();
            if (text.StartsWith('/'))
            {
                //TODO: Command
                ServerSender.SendChatMessage(clientID, "You are not an administrator!", false);
                return;
            }

            text = "[" + NetworkManager.Find(clientID).Name + "]:" + text;
            ServerSender.SendChatMessageToAll(text, false);
        }

        [MessageHandler((ushort)ClientToServerPacket.AttackEntityRequest)]
        private static void RecieveAttackEntityRequest(ushort clientID, Message message)
        {
            int victimId = message.GetInt();
            int attackerId = message.GetInt();

            Entity victim = FindObjectsOfType<Entity>().First(x => x.Id == victimId);
            Entity attacker = FindObjectsOfType<Entity>().First(x => x.Id == attackerId);

            if (!attacker.GetTargetables().Contains(victim))
            {
                ServerSender.SendAlert(clientID, "You can't attack that unit!");
                return;
            }

            victim.OnDamaged(attacker.damage);
            attacker.canMove = false;
            ServerSender.DamageEntityByEntity(victim, attacker);
        }
        
    }
}