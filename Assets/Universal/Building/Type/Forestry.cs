using ServerSide;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forestry : AbstractBuilding, IResourceStorage
{
    public Forestry(ushort owner, Vector3Int position) : base(owner, position) 
    {
        BuildingStorage = new List<ResourceStorage>();
        foreach (ResourceHolder holder in GetDefinition().StorageCapacity)
        {
            BuildingStorage.Add(new ResourceStorage(this, holder.type, new Dictionary<int, double>()
            {
                { 1, holder.Value }
            }));
        }
    }

    public override BuildingType BuildingType => BuildingType.Forestry;

    public List<ResourceStorage> Storage => BuildingStorage;

    public List<ResourceStorage> BuildingStorage;

    public override void OnTurnCycleEnded()
    {
        double produce = GetDefinition().ProduceLevel.Find(x => x.level == Level).value;
        produce -= Storage[0].AddSafe(produce);
        if (produce > 0) //Remained some resource
        {
            Owner.TryStoreResource(GetDefinition().produceType, produce);
        }
    }
}
