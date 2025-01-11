using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerSide
{
    public class ServerPlayer
    {
        public ushort PlayerId { get; private set; }
        public string Name { get; private set; }
        public Color Color { get; private set; }
        public bool IsReady = false;
        public bool IsMainSceneLoaded = false;
        public bool isFirstPlace = true; //Whenever this player builded something or not

        public List<AbstractBuilding> Buildings { get; private set; } = new List<AbstractBuilding>();
        //How many times a building bought by the player
        public Dictionary<BuildingType, int> BuildingBought { get; private set; } = new();
        public List<Entity> entities = new();

        public ServerPlayer(ushort playerId, string name, Color color)
        {
            PlayerId = playerId;
            Name = name;
            Color = color;
            NetworkManager.players.Add(this);
        }

        public List<ResourceHolder> GetAvaibleResources()
        {
            List<ResourceHolder> resources = new List<ResourceHolder>();
            foreach (AbstractBuilding building in Buildings)
            {
                if(building is IResourceStorage storage)
                {
                    foreach (ResourceStorage res in storage.Storage)
                    {
                        ResourceHolder holder = resources.GetOrCreate(res.Type);
                        holder.Value += res.Amount;
                    }
                }
            }
            return resources;
        }

        /// <summary>
        /// Tries to store these amount of resource in some building
        /// </summary>
        /// <param name="type"></param>
        /// <param name="amount"></param>
        public void TryStoreResource(ResourceType type, double amount)
        {
            double remained = amount;
            foreach (AbstractBuilding building in Buildings)
            {
                if (remained <= 0) break;

                if (building is IResourceStorage storage)
                {
                    foreach (ResourceStorage res in storage.Storage)
                    {
                        if(res.Type == type && res.Amount != res.MaxAmountAtLevel[building.Level])
                        {
                            remained -= res.AddSafe(remained);
                        }
                    }
                }
            }
        }

        public bool CouldStoreResource(ResourceType type, double amount)
        {
            double remained = amount;
            foreach (AbstractBuilding building in Buildings)
            {
                if (remained <= 0) break;

                if (building is IResourceStorage storage)
                {
                    foreach (ResourceStorage res in storage.Storage)
                    {
                        if (res.Type == type && res.Amount != res.MaxAmountAtLevel[building.Level])
                        {
                            remained -= res.CouldAddTillMax();
                        }
                    }
                }
            }

            return remained <= 0;
        }

        public Dictionary<ResourceType, double> GetResourceGeneratePerTurn()
        {
            Dictionary<ResourceType, double> generate = new Dictionary<ResourceType, double>();
            foreach (AbstractBuilding building in Buildings)
            {
                BuildingDefinition definition = building.GetDefinition();
                if (definition.isProducer)
                {
                    ResourceType type = definition.produceType;
                    if (generate.ContainsKey(type))
                    {
                        generate[type] += definition.ProduceLevel.Find(x => x.level == building.Level).value;
                    }
                    else
                    {
                        generate.Add(type, definition.ProduceLevel.Find(x => x.level == building.Level).value);
                    }
                }
            }

            foreach(Entity entity in entities)
            {
                foreach(ResourceType type in entity.Definition.GetUpkeep().Keys)
                {
                    double value = entity.Definition.GetUpkeep()[type];
                    if (!generate.ContainsKey(type))
                    {
                        generate.Add(type, -value);
                    }
                    else
                    {
                        generate[type] -= value;
                    }
                }
            }

            return generate;
        }

        public void IncrementBuildingBought(BuildingType type)
        {
            if (BuildingBought.ContainsKey(type))
            {
                BuildingBought[type] += 1;
            }
            else BuildingBought.Add(type, 1);
        }

        public AbstractBuilding GetBuildingByID(Guid ID)
        {
            return Buildings.Find(x => x.ID == ID);
        }

        /// <summary>
        /// This always check if the player has enough resource to pay it
        /// </summary>
        /// <param name="cost"></param>
        /// <returns>False if couldn't pay</returns>
        public bool PayResources(Dictionary<ResourceType, double> cost)
        {
            #region Has enough 
            List<ResourceHolder> resources = GetAvaibleResources();
            foreach (ResourceType resType in cost.Keys)
            {
                ResourceHolder holder = resources.Get(resType);
                if (holder == null || holder.Value < cost[resType])
                {
                    return false;
                }
            }
            #endregion

            foreach (ResourceType resType in cost.Keys)
            {
                double needToPay = cost[resType];
                foreach (AbstractBuilding building in Buildings)
                {
                    if (needToPay <= 0) break;

                    if(building is IResourceStorage storage)
                    {
                        ResourceStorage actualStorage = storage.Storage.Get(resType);
                        if (actualStorage == null || actualStorage.Amount <= 0) continue;

                        //If we have less stored than we need
                        //we take all of it from the building
                        if(needToPay >= actualStorage.Amount)
                        {
                            needToPay -= actualStorage.Amount;
                            actualStorage.Amount = 0;
                        }
                        else //We need to play less than we have
                        {
                            actualStorage.Amount -= needToPay;
                            needToPay = 0;
                        }
                    }
                }

                //None of the buildings could pay for it :(
                if(needToPay > 0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool PayResources(params KeyValuePair<ResourceType, double>[] amount)
        {
            return PayResources(amount.ToDictionary(x => x.Key, x => x.Value));
        }
    }
}
