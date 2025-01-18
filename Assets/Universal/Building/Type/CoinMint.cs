using ServerSide;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinMint : AbstractBuilding, IDisable
{
    private bool active = true;

    public CoinMint(ushort owner, Vector3Int position) : base(owner, position) 
    {
    }

    public override BuildingType BuildingType => BuildingType.CoinMint;

    public bool IsActive()
    {
        return active;
    }

    public void SetActive(bool IsActive)
    {
        active = IsActive;
    }

    public override void OnTurnCycleEnded()
    {
        if (!IsActive()) return;

        Owner.TryStoreResource(GetDefinition().produceType, GetDefinition().ProduceLevel.Find(x => x.level == Level).value);
    }
}
