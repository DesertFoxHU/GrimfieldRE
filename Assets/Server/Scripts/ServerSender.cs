using Riptide;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace ServerSide
{
    public static class ServerSender
    {
        public static void SendAlert(ushort clientID, string message)
        {
            NetworkManager.Instance.Server.Send(
                    Message.Create(MessageSendMode.unreliable, ServerToClientPacket.SendAlert).Add(message),
                    clientID);
        }

        /// <summary>
        /// Sends everybody a request to render the new building
        /// </summary>
        /// <param name="building"></param>
        public static void SendNewBuilding(ServerPlayer owner, AbstractBuilding building)
        {
            Message message = Message.Create(MessageSendMode.reliable, ServerToClientPacket.NewBuildingAdded);
            message.Add(owner.PlayerId);
            message.Add(building.ID);
            message.Add(building.BuildingType.ToString());
            message.Add(building.Position);
            message.Add(building.Level);

            NetworkManager.Instance.Server.SendToAll(message);
            ServerSender.UpdateResourceCost(owner, building.BuildingType);
        }

        public static void UpdatePlayerResource(ServerPlayer player, List<ResourceHolder> summerized, Dictionary<ResourceType, double> perTurn)
        {
            Message message = Message.Create(MessageSendMode.unreliable, ServerToClientPacket.PlayerResourceUpdate);
            message.Add(summerized.Count);
            foreach (ResourceHolder holder in summerized)
                message.Add(holder.type.ToString()).Add(holder.Value).Add(perTurn.ContainsKey(holder.type) ? perTurn[holder.type] : 0).Add(holder.MaxValue);

            NetworkManager.Instance.Server.Send(message, player.PlayerId);
        }

        public static void SyncPlayers()
        {
            foreach(ServerPlayer player in NetworkManager.players)
            {
                //The first half of the message is this player's ServerPlayer info
                Message message = Message.Create(MessageSendMode.reliable, ServerToClientPacket.SyncPlayers);
                message.Add(NetworkManager.players.Count);
                message.Add(player.PlayerId);
                message.Add(player.Name);
                message.Add(player.Color);
                message.Add(NetworkManager.players.Count - 1); //-1 because we already added the owner's ServerPlayer
                foreach (ServerPlayer otherPlayer in NetworkManager.players)
                    if (otherPlayer.PlayerId != player.PlayerId) message.Add(otherPlayer.PlayerId).Add(otherPlayer.Name).Add(otherPlayer.Color);

                NetworkManager.Instance.Server.Send(message, player.PlayerId);
            }
        }

        public static void TurnChange(ServerPlayer currentPlayer, int turnCycle)
        {
            Message message = Message.Create(MessageSendMode.reliable, ServerToClientPacket.TurnChange);
            message.Add(currentPlayer.PlayerId);
            message.Add(turnCycle);
            NetworkManager.Instance.Server.SendToAll(message);
        }

        public static void UpdateResourceCost(ServerPlayer player, BuildingType type)
        {
            Message message = Message.Create(MessageSendMode.reliable, ServerToClientPacket.UpdateBuildingCost);
            message.Add(type.ToString());
            message.Add(player.BuildingBought.ContainsKey(type) ? player.BuildingBought[type] : 0);
            NetworkManager.Instance.Server.Send(message, player.PlayerId);
        }

        public static void RenderTerritory(ServerPlayer owner, AbstractBuilding building)
        {
            ServerTerritory territory = building.territory;
            foreach (int index in territory.ClaimedLand.Keys)
            {
                List<Vector3Int> land = territory.ClaimedLand[index];
                Message message = Message.Create(MessageSendMode.reliable, ServerToClientPacket.RenderTerritory);
                message.Add(owner.PlayerId);
                message.Add(territory.ID);
                message.Add(index);
                message.Add(land.Count);
                foreach (Vector3Int v3 in land)
                {
                    message.Add(v3);
                }
                NetworkManager.Instance.Server.SendToAll(message);
            }
        }

        public static void SendChatMessageToAll(string text, bool forceOpen)
        {
            Message response = Message.Create(MessageSendMode.reliable, ServerToClientPacket.SendMessage);
            response.Add(text);
            response.Add(forceOpen);
            NetworkManager.Instance.Server.SendToAll(response);
        }

        public static void SendChatMessage(ushort clientID, string text, bool forceOpen)
        {
            Message response = Message.Create(MessageSendMode.reliable, ServerToClientPacket.SendMessage);
            response.Add(text);
            response.Add(forceOpen);
            NetworkManager.Instance.Server.Send(response, clientID);
        }

        public static void DestroyEntity(Entity entity)
        {
            Message message = Message.Create(MessageSendMode.reliable, ServerToClientPacket.DestroyEntity);
            message.Add(entity.Id);
            NetworkManager.Instance.Server.SendToAll(message);
        }

        public static void DamageEntityByEntity(Entity victim, Entity attacker)
        {
            Message message = Message.Create(MessageSendMode.reliable, ServerToClientPacket.RenderAttackEntity);
            message.Add(victim.Id);
            message.Add(attacker.Id);
            message.Add(victim.health);
            NetworkManager.Instance.Server.SendToAll(message);
        }

        public static void DestroyBuilding(AbstractBuilding building)
        {
            Message message = Message.Create(MessageSendMode.reliable, ServerToClientPacket.DestroyBuilding);
            message.Add(building.ID);
            NetworkManager.Instance.Server.SendToAll(message);

            building.owner.Buildings.Remove(building);
        }

        public static void InitChunkLoadingState(ServerPlayer player, int chunkCount)
        {
            Message message = Message.Create(MessageSendMode.reliable, ServerToClientPacket.InitChunkLoadingState);
            message.Add(chunkCount);
            NetworkManager.Instance.Server.Send(message, player.PlayerId);
        }
    }
}