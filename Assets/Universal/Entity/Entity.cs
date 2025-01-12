using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Linq;

public class Entity : MonoBehaviour
{
    public EntityDefinition Definition { get; private set; }
    public EntityType Type
    {
        get => Definition.Type;
    }
    public int Id { get; protected set; }
    public bool IsInitialized { get; protected set; } = false;
    public Vector3Int Position { get; set; }
    public ushort? OwnerId { get; protected set; } = null;
    public double health;
    public double damage;
    public double speed;

    private List<Vector3Int> lastDrawn = new List<Vector3Int>();
    private HashSet<Vector3Int> lastCanMove = new HashSet<Vector3Int>();

    private MaterialInstancing secondaryTexture;
    [HideInInspector] public bool canMove;
    public List<Entity> LastTargetables { get; private set; } = new();
    public Image healthBar;

    #region 
    public int lastTurnWhenMoved;
    public AbstractBuilding claiming;
    public bool canClaimBuilding = false;
    #endregion

    public float HealthPercentage
    {
        get => (float)health / (float)Definition.Health[0];
    }

    public void Awake()
    {
        secondaryTexture = GetComponent<MaterialInstancing>();
    }

    [ContextMenu("Initialize (DEBUG)")]
    private void InitializeDebug()
    {
        Tilemap map = FindAnyObjectByType<Tilemap>();
        Vector3Int position = map.ToVector3Int(this.transform.position);
        EntityDefinition definition = FindAnyObjectByType<DefinitionRegistry>().Find(initType);
        Initialize(position, definition, 0);
    }

    public void Initialize(Vector3Int Position, EntityDefinition definition, int Id)
    {
        Definition = definition;
        this.Position = Position;
        health = definition.Health[0];
        damage = definition.Damage[0];
        speed = definition.Speed[0];
        this.Id = Id;
        canMove = false;
        RefreshHealthbar();

        Debug.Log($"Initialized entity: Type:{Definition.Type} Pos:{Position} ID:{Id}");
        IsInitialized = true;
    }

    public void SetOwner(ushort clientID)
    {
        this.OwnerId = clientID;
    }

    public void SetColor(Color color)
    {
        secondaryTexture.input = color;
        secondaryTexture.ChangePropertyBlock();
    }

    public List<Entity> GetTargetables()
    {
        List<Entity> entities = FindObjectsByType<Entity>(FindObjectsSortMode.None).ToList();
        entities.RemoveAll(x => x.Id == this.Id || x.OwnerId == this.OwnerId);

        List<Entity> final = new List<Entity>();
        foreach (Entity entity in entities)
        {
            int distX = Mathf.Abs(Position.x - entity.Position.x);
            int distY = Mathf.Abs(Position.y - entity.Position.y);
            
            if(distX <= entity.Definition.attackRange[0] && distY <= entity.Definition.attackRange[0]) final.Add(entity);
        }
        return final;
    }

    public void DrawTargetables()
    {
        ClearTargetables();

        List<Entity> targets = GetTargetables();
        foreach(Entity entity in targets)
        {
            entity.gameObject.GetChildrenByName("AttackableMarker").GetComponent<SpriteRenderer>().enabled = true;
            LastTargetables.Add(entity);
        }
    }

    public void ClearTargetables()
    {
        foreach (Entity last in LastTargetables)
        {
            last.gameObject.GetChildrenByName("AttackableMarker").GetComponent<SpriteRenderer>().enabled = false;
        }
        LastTargetables.Clear();
    }

    public void RefreshHealthbar()
    {
        healthBar.fillAmount = HealthPercentage;
    }

    #region Debug
    public bool isDebug;
    [Conditional("isDebug", true)] public Entity toAttack;
    [Conditional("isDebug", true)] public EntityType initType = EntityType.Skeleton;
    [Conditional("isDebug", true)] public bool trigger;
    public void Update()
    {
        if (!isDebug) return;

        if(toAttack != null && trigger)
        {
            Attack(toAttack);
            trigger = false;
        }
    }
    #endregion

    public void Attack(Entity victim)
    {
        Tilemap map = GameObject.FindGameObjectWithTag("GameMap").GetComponent<Tilemap>();
        Vector3 original = this.transform.position;
        Vector3 target = map.ToVector3Center(victim.Position);

        this.GetComponent<Animator>().SetTrigger("Attack");
        StartCoroutine(AttackAnimation(victim, original, this.gameObject, target, Definition.animatorValues.attackStartTime));
    }

    public IEnumerator AttackAnimation(Entity victim, Vector3 original, GameObject objectToMove, Vector3 end, float seconds)
    {
        yield return MoveOverSeconds(objectToMove, end, seconds);
        if(victim != null)
        {
            victim.GetComponent<Animator>().SetTrigger("Hurt");
            victim.RefreshHealthbar();
        }
        yield return new WaitForSeconds(Definition.animatorValues.attackEndTime);
        yield return MoveOverSeconds(objectToMove, original, seconds);
    }

    public IEnumerator MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        objectToMove.transform.position = end;
    }

    #region ServerSide Inner Events
    public virtual void OnGotTurn()
    {
        canClaimBuilding = true;
        if(claiming != null)
        {
            ServerSide.ServerSender.DestroyBuilding(claiming);
        }
    }

    public virtual void OnMoved(Vector3Int from, Vector3Int to) { }

    public virtual void OnDamaged(double damage) 
    {
        health -= damage;
        if(health <= 0)
        {
            ServerSide.EntityManager.DestroyEntity(this);
            return;
        }
        RefreshHealthbar();
    }

    public virtual void OnUpkeepFailedToPay() 
    {
        ServerSide.EntityManager.DestroyEntity(this);
    }
    #endregion

    #region Movement
    public void CalculateMovementRange()
    {
        WeightGraph graph = new WeightGraph(this);
        lastCanMove = graph.GetMovementRange(this);
    }

    public void DrawNavigation()
    {
        Tilemap map = GameObject.FindGameObjectWithTag("GameMap").GetComponent<Tilemap>();
        CalculateMovementRange();

        Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        Vector3 botLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));

        topRight.Set(topRight.x + 3f, topRight.y + 3f, topRight.z);
        botLeft.Set(botLeft.x - 3f, botLeft.y - 3f, botLeft.z);

        Vector3Int topRightInt = map.ToVector3Int(topRight);
        Vector3Int botLeftInt = map.ToVector3Int(botLeft);

        Color outside = new Color(87 / 255f, 87 / 255f, 87 / 255f, 235 / 255f);

        for(int x = botLeftInt.x; x < topRightInt.x; x++)
        {
            for (int y = botLeftInt.y; y < topRightInt.y; y++)
            {
                Vector3Int v3 = new Vector3Int(x, y, botLeftInt.z);
                if (lastCanMove.Contains(v3)) continue;

                map.SetColor(v3, outside);
                lastDrawn.Add(v3);
            }
        }
        map.RefreshAllTiles();
    }

    public void ClearDraw()
    {
        Tilemap map = GameObject.FindGameObjectWithTag("GameMap").GetComponent<Tilemap>();
        foreach (Vector3Int v3 in lastDrawn)
        {
            map.SetColor(v3, Color.white);
        }
        lastDrawn.Clear();
    }
    #endregion

    #region ClientSide
    public void ClientMoveToRequest(Vector3Int to)
    {
        if (!canMove)
        {
            FindAnyObjectByType<MessageDisplayer>().SetMessage($"This unit has been already moved.");
            return;
        }

        CalculateMovementRange();
        if (!lastCanMove.Contains(to))
        {
            FindAnyObjectByType<MessageDisplayer>().SetMessage($"This unit can't move there.");
            return;
        }

        Message message = Message.Create(MessageSendMode.reliable, ClientToServerPacket.MoveEntity);
        message.Add(this.Id);
        message.Add(Position);
        message.Add(to);
        NetworkManager.Instance.Client.Send(message);
    }

    public void ClientAttackRequest(Entity attacker)
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerPacket.AttackEntityRequest);
        message.Add(this.Id);
        message.Add(attacker.Id);
        NetworkManager.Instance.Client.Send(message);
    }
    #endregion
}
