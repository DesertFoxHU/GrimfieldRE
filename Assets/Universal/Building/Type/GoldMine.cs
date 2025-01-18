using ServerSide;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldMine : AbstractBuilding
{
    public GoldMine(ushort owner, Vector3Int position) : base(owner, position) 
    {
    }

    public override BuildingType BuildingType => BuildingType.GoldMine;

    public override void OnTurnCycleEnded()
    {
        Owner.TryStoreResource(GetDefinition().produceType, GetDefinition().ProduceLevel.Find(x => x.level == Level).value);
    }
}
