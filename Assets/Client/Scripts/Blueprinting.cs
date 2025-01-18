using ClientSide;
using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Blueprinting : MonoBehaviour
{
    public GameObject blueprint;
    [HideInInspector] public BuildingDefinition current = null;
    private Tilemap map;
    private SpriteRenderer spriteRenderer;

    public void Start()
    {
        map = GameObject.FindGameObjectWithTag("GameMap").GetComponent<Tilemap>();
        spriteRenderer = blueprint.GetComponent<SpriteRenderer>();
    }

    public bool IsBlueprinting() { return current != null; }

    public void Cancel() { current = null; Hide(); }

    public void Show()
    {
        blueprint.SetActive(true);
    }

    public void Hide()
    {
        blueprint.SetActive(false);
    }

    public void SetPosition(Vector3 vector3)
    {
        Vector3Int pos = map.ToVector3Int(vector3);
        if (!map.HasTile(pos)) Hide();
        else
        {
            Show();
            Vector3 newVector = map.ToVector3(pos);
            blueprint.transform.localPosition = new Vector3(newVector.x + 0.5f, newVector.y + 0.5f, -1f);
        }
    }

    public void ChangeBlueprint(BuildingDefinition definition)
    {
        this.current = definition;
        this.spriteRenderer.sprite = definition.spritesLevel[0];
        Show();
    }

    public void Update()
    {
        if (IsBlueprinting())
        {
            if (Utils.IsPointerOverUIElement()) Hide();
            else Show();

            if (Input.GetMouseButtonDown(1)) 
            {
                Cancel();
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 worldPoint = ray.GetPoint(-ray.origin.z / ray.direction.z);

            SetPosition(worldPoint);

            bool canBuild = true;

            Vector3Int pos = map.ToVector3Int(worldPoint);
            TileDefinition definition = map.GetTile<GrimfieldTile>(pos).definition;
            if (definition == null)
            {
                Debug.LogError($"Definition is null for: {pos}, mapReference: {map}");
                return;
            }

            if (!current.placeable.Contains(definition.type))
            {
                spriteRenderer.color = Color.red;
                canBuild = false;
            }
            else spriteRenderer.color = Color.white;

            if (canBuild && Input.GetMouseButtonDown(0))
            {
                Message message = Message.Create(MessageSendMode.unreliable, ClientToServerPacket.RequestBuild);
                message.Add(worldPoint);
                message.Add("" + current.type);
                NetworkManager.Instance.Client.Send(message);
                Cancel();
            }
        }
    }
}
