using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using InfoPanel;

public class CursorAction : MonoBehaviour
{
    private static Tilemap map;
    [HideInInspector] public Entity selectedEntity;
    public GameObject selectedEntityMarker;

    void Start()
    {
        map = GameObject.FindGameObjectWithTag("GameMap").GetComponent<Tilemap>();
        selectedEntityMarker.SetActive(false);
    }

    void Update()
    {
        if (Utils.IsPointerOverUIElement()) return;
        if (map == null) return;

        if (Input.GetMouseButtonDown(0)) FireAction(MouseClickType.LeftClick);
        else if (Input.GetMouseButtonDown(1)) FireAction(MouseClickType.RightClick);
        else if(Input.GetMouseButtonDown(2)) FireAction(MouseClickType.MiddleClick);
    }

    private void FireAction(MouseClickType type)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 worldPoint = ray.GetPoint(-ray.origin.z / ray.direction.z);
        Vector3Int pos = map.ToVector3Int(worldPoint);
        Vector3 worldPos = map.ToVector3(pos);
        worldPos = new Vector3(worldPos.x + .5f, worldPos.y + .5f, -1.2f);

        if (!map.HasTile(pos)) return;

        foreach(Entity entity in GameObject.FindObjectsByType<Entity>(FindObjectsSortMode.None))
        {
            if(entity.Position.x == pos.x && entity.Position.y == pos.y)
            {
                //Player clicked on an enemy unit, so this is an attack
                if(type == MouseClickType.LeftClick && selectedEntity != null && entity.OwnerId != selectedEntity.OwnerId)
                {
                    if (!selectedEntity.GetTargetables().Contains(entity))
                    {
                        FindAnyObjectByType<MessageDisplayer>().SetMessage("You can't reach this unit!");
                        return;
                    }

                    if (!selectedEntity.canMove)
                    {
                        FindAnyObjectByType<MessageDisplayer>().SetMessage("This unit has already moved/attacked in this turn!");
                        return;
                    }

                    entity.ClientAttackRequest(selectedEntity);
                    selectedEntity.ClearDraw();
                    selectedEntity.ClearTargetables();
                    selectedEntity = null;
                    selectedEntityMarker.SetActive(false);
                    return;
                }

                if(type == MouseClickType.LeftClick)
                {
                    entity.DrawNavigation();
                    entity.DrawTargetables();
                    selectedEntity = entity;
                    selectedEntityMarker.SetActive(true);
                    selectedEntityMarker.transform.localPosition = worldPos;
                    FindAnyObjectByType<InfoWindow>().Load(entity);
                    return;
                }
                
                FindAnyObjectByType<InfoWindow>().Load(entity);
                return;
            }
        }

        if(selectedEntity != null)
        {
            if(type == MouseClickType.RightClick)
            {
                selectedEntity.ClearDraw();
                selectedEntity.ClearTargetables();
                selectedEntity = null;
                selectedEntityMarker.SetActive(false);
                return;
            }

            if(selectedEntity.OwnerId == NetworkManager.Instance.ClientPlayer.ClientID) selectedEntity.ClientMoveToRequest(pos);
            selectedEntity.ClearDraw();
            selectedEntity.ClearTargetables();
            selectedEntity = null;
            selectedEntityMarker.SetActive(false);
        }

        foreach (Transform child in map.transform)
        {
            string name = child.name;
            if (name.StartsWith("Building"))
            {
                Vector3Int buildingPos = map.ToVector3Int(child.position);
                if(buildingPos.x == pos.x && buildingPos.y == pos.y)
                {
                    ushort clientID = ushort.Parse(name.Split('_')[1]);
                    if (clientID != NetworkManager.Instance.ClientPlayer.ClientID) break;

                    string rawGuid = name.Split('_')[2];
                    ClientSender.SendBuildingFetchRequest(rawGuid, type);
                    return;
                }
            }
        }

        FindAnyObjectByType<InfoWindow>().Load(pos);
    }
}
