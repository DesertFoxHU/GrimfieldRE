using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerSide
{
    public class TurnHandler
    {
        public List<ServerPlayer> turnOrder;
        public int currentIndex = 0;
        /// <summary>
        /// One cycle means every player had a turn once
        /// </summary>
        public int turnCycleCount = 0;

        public TurnHandler()
        {
            turnOrder = new List<ServerPlayer>(NetworkManager.players);
            turnOrder.Shuffle();
        }

        /// <summary>
        /// The current turn's owner is ended their turn
        /// </summary>
        public void TurnEnded()
        {
            currentIndex++;
            if (currentIndex > turnOrder.Count - 1)
            {
                currentIndex = 0;
                OnNewTurnCycle();
            }

            ServerPlayer currentPlayer = turnOrder[currentIndex];
            foreach (Entity entity in currentPlayer.entities)
            {
                entity.canMove = true;
                entity.OnGotTurn();
            }

            ServerSender.TurnChange(turnOrder[currentIndex], turnCycleCount);
        }

        /// <summary>
        /// Called when every player had thier turn
        /// </summary>
        public void OnNewTurnCycle()
        {
            turnCycleCount++;
            foreach (ServerPlayer player in turnOrder)
            {
                foreach(AbstractBuilding building in player.Buildings)
                {
                    building.OnTurnCycleEnded();
                    if(building.GetDefinition() != null)
                    {
                        if(building.GetDefinition().Upkeep != null || building.GetDefinition().Upkeep.Count > 0)
                        {
                            BuildingDefinition buildDef = building.GetDefinition();
                            if (!player.PayResources(buildDef.Upkeep.ToDictionary(x => x.type, x => x.Value), true))
                            {
                                
                            }
                        }
                    }
                }

                foreach(Entity entity in player.entities)
                {
                    if (!player.PayResources(entity.Definition.GetUpkeep(), false))
                    {
                        entity.OnUpkeepFailedToPay();
                    }
                }
            }
        }

        /// <summary>
        /// The ClientID's the current turn player
        /// </summary>
        /// <returns></returns>
        public ushort GetCurrentTurnOwnerID()
        {
            return turnOrder[currentIndex].PlayerId;
        }
    }
}