using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ServerSide
{
    public class EntityManager
    {
        public static bool OnMoveTo(ServerPlayer player, Tilemap map, Entity entity, Vector3Int newPosition)
        {
            entity.lastTurnWhenMoved = GameController.Instance.turnHandler.turnCycleCount;

            AbstractBuilding building = NetworkManager.GetAllBuilding().Find(x => x.Position.x == newPosition.x && x.Position.y == newPosition.y);
            if(building != null && building.owner.PlayerId != player.PlayerId)
            {
                entity.claiming = building;
                entity.canClaimBuilding = false;
                ServerSender.SendChatMessage(building.owner.PlayerId, "Your building is under attack: " + building.GetDefinition().name, true);
            }
            else
            {
                entity.claiming = null;
                entity.canClaimBuilding = false;
            }

            return true;
        }

        public static void DestroyEntity(Entity entity)
        {
            GameObject reference = entity.gameObject;
            ServerSender.DestroyEntity(entity);
            if(reference != null) UnityEngine.Object.Destroy(reference);
        }

        /// <summary>
        /// Calculates a new Unique ID (incremental) for an entity
        /// </summary>
        /// <returns>Returns the first non exist ID</returns>
        public static int GetNextEntityId()
        {
            int Id = 0;
            foreach(Entity entity in UnityEngine.Object.FindObjectsByType<Entity>(FindObjectsSortMode.None))
            {
                if(entity.Id == Id)
                {
                    Id++;
                }
            }

            return Id;
        }
    }
}
