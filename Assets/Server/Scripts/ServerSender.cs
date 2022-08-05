using RiptideNetworking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ServerSide
{
    public static class ServerSender
    {
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
        }

        public static void UpdatePlayerResource(ServerPlayer player, List<ResourceHolder> summerized)
        {
            Message message = Message.Create(MessageSendMode.unreliable, ServerToClientPacket.PlayerResourceUpdate);
            message.Add(summerized.Count);
            foreach (ResourceHolder holder in summerized)
                message.Add(holder.type.ToString()).Add(holder.Value);

            NetworkManager.Instance.Server.Send(message, player.PlayerId);
        }
    }
}