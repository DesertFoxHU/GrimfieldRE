using ServerSide;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrack : AbstractBuilding
{
    public Barrack(ushort owner, Vector3Int position) : base(owner, position) 
    {
    }

    public override BuildingType BuildingType => BuildingType.Barrack;

    public void InvokeRecruit()
    {

    }

    public override void OnTurnCycleEnded()
    {
        
    }
}
