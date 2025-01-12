using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tile/BuildingDefinition", fileName = "BuildingDefinition")]
public class BuildingDefinition : ScriptableObject
{
    public BuildingType type;
    public Sprite[] spritesLevel;
    public List<TileType> placeable;
    public string description;

    public List<ResourceHolder> ResourceCost;
    public List<ResourceHolder> IncreasePerBuy;
    public List<ResourceHolder> Upkeep;

    #region Storage
    public bool hasProductStorage;
    public List<ResourceHolder> StorageCapacity;
    #endregion

    #region ProducerBuilding
    public bool isProducer;
    [Conditional("isProducer", true)] public ResourceType produceType;
    /*[Conditional("isProducer", true)]*/ public List<ValuePair> ProduceLevel;
    #endregion

    #region Territory
    [Space] public bool canClaimTerritory;
    [Conditional("canClaimTerritory", true)] public int territoryClaimRange;
    #endregion

    #region Exchanger
    [Space] public bool isExchanger;
    public List<ExchangeRate> ExchangeFrom;
    public List<ExchangeRate> ExchangeTo;
    #endregion

    public List<EntityType> canRecruit;

    public Sprite GetSpriteByLevel(int level)
    {
        return spritesLevel[level-1];
    }

    public List<EntityDefinition> GetRecruitable()
    {
        List<EntityDefinition> list = new List<EntityDefinition>();
        foreach(EntityType type in canRecruit)
            list.Add(FindAnyObjectByType<DefinitionRegistry>().Find(type));
        return list;
    }

    public Dictionary<ResourceType, double> GetBuildingCost(int boughtCount)
    {
        Dictionary<ResourceType, double> cost = new Dictionary<ResourceType, double>();
        foreach (ResourceHolder holder in ResourceCost)
        {
            if (!cost.ContainsKey(holder.type))
            {
                cost.Add(holder.type, holder.Value);
            }
            else cost[holder.type] += holder.Value;
        }

        if (boughtCount != 0)
        {
            foreach (ResourceHolder holder in IncreasePerBuy)
            {
                if (!cost.ContainsKey(holder.type))
                {
                    cost.Add(holder.type, holder.Value * boughtCount);
                }
                else cost[holder.type] += holder.Value * boughtCount;
            }
        }
        return cost;
    }
}

[System.Serializable]
public struct ValuePair
{
    public int level;
    public double value;
}

[System.Serializable]
public struct ExchangeRate
{
    public int level;
    public ResourceType type;
    public double amount;
}
