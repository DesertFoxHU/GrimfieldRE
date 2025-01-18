using Newtonsoft.Json;
using ServerSide;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public abstract class AbstractBuilding
{
    public readonly Guid ID = Guid.NewGuid();
    public Vector3Int Position { get; private set; }
    public int Level { get; private set; }
    public ushort OwnerId { get; private set; }
    #region ServerSide
    [JsonIgnore] public List<Vector3Int> ClaimedLand { get; private set; } = new List<Vector3Int>();
    #endregion

    public AbstractBuilding(ushort ownedId, Vector3Int position)
    {
        this.OwnerId = ownedId;
        Position = position;
        Level = 1;
        if (NetworkChecker.IsServer()) OnClaimLand(GameObject.FindGameObjectWithTag("GameMap").GetComponent<Tilemap>());
    }

    public ServerPlayer Owner { get => ServerSide.NetworkManager.Find(OwnerId); }

    public static Type GetClass(BuildingType type)
    {
        if (type == BuildingType.Village) return typeof(Village);
        else if (type == BuildingType.Forestry) return typeof(Forestry);
        else if (type == BuildingType.Orchard) return typeof(Orchard);
        else if (type == BuildingType.Quarry) return typeof(Quarry);
        else if (type == BuildingType.GoldMine) return typeof(GoldMine);
        else if (type == BuildingType.CoinMint) return typeof(CoinMint);

        else if (type == BuildingType.Barrack) return typeof(Barrack);
        else return null;
    }

    public void RemoveClaim(Tilemap map)
    {
        foreach(Vector3Int pos in ClaimedLand)
        {
            if (!map.HasTile(pos)) continue;

            map.GetTile<GrimfieldTile>(pos).isClaimed = false;
        }
        ClaimedLand.Clear();
    }

    public virtual void OnClaimLand(Tilemap map)
    {
        if (!GetDefinition().canClaimTerritory) return;

        OnClaimLand(map, GetDefinition().territoryClaimRange);
    }

    public void OnClaimLand(Tilemap map, int range)
    {
        if (!GetDefinition().canClaimTerritory) return;

        RemoveClaim(map);
        List<Vector3Int> possibility = map.GetTileRange(Position, range);
        foreach (Vector3Int pos in possibility)
        {
            if (!map.HasTile(pos)) continue;

            GrimfieldTile tile = map.GetTile<GrimfieldTile>(pos);
            if (tile == null)
            {
                Debug.LogError($"Tile was not a grimfieldTile: {pos}");
                continue;
            }

            if (tile.isClaimed) continue;
            tile.isClaimed = true;
            ClaimedLand.Add(pos);
        }
    }

    /// <summary>
    /// Called when every player had thier turn
    /// </summary>
    public virtual void OnTurnCycleEnded() { }

    public abstract BuildingType BuildingType { get; }

    public BuildingDefinition GetDefinition()
    {
        return DefinitionRegistry.Instance.Find(BuildingType);
    }
}
