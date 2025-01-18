using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ServerToClientPacket : ushort
{
    SendAlert = 1,
    LoadLobby,
    UpdateLobby,
    LoadGameScene,
    InitChunkLoadingState,
    LoadChunk,
    NewBuildingAdded,
    PlayerResourceUpdate,
    SyncPlayers,
    TurnChange,
    UpdateBuildingCost,
    FetchBuildingDataResponse,
    SpawnEntity,
    MoveEntity,
    SendMessage,
    DestroyEntity,
    RenderAttackEntity,
    DestroyBuilding,
    UpdateTerritory
}

public enum ClientToServerPacket : ushort
{
    JoinLobby = 1,
    ChangeReadyStatus,
    RequestBuild,
    MainGameLoaded,
    NextTurn,
    FetchBuildingData,
    BuyEntity,
    MoveEntity,
    SendMessage,
    AttackEntityRequest,
}

