using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ResourceHolder is capable of storing a single type of Resource, but its not suitable for holding all Resource type
/// Instead use ResourceInventory for tracking every Resource
/// </summary>
[System.Serializable]
public class ResourceHolder
{
    public ResourceType type;
    public double Value = 0;
    public double MaxValue = 0;

    public ResourceHolder()
    {
    }

    public ResourceHolder(ResourceType type)
    {
        this.type = type;
    }

    public ResourceHolder(ResourceType type, double value)
    {
        this.type = type;
        Value = value;
    }

    public ResourceHolder(ResourceType type, double value, double maxValue) : this(type, value)
    {
        MaxValue = maxValue;
    }
}

public static class ResourceHolderExtensions
{
    public static ResourceHolder GetOrCreate(this List<ResourceHolder> list, ResourceType type)
    {
        ResourceHolder holder = list.Find(x => x.type == type);
        if (holder == null)
        {
            holder = new ResourceHolder(type, 0);
            list.Add(holder);
        }
        return holder;
    }

    public static ResourceHolder Get(this List<ResourceHolder> list, ResourceType type)
    {
        ResourceHolder holder = list.Find(x => x.type == type);
        return holder;
    }
}
