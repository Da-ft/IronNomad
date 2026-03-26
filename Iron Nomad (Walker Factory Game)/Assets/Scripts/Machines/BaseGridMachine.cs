using UnityEngine;

public abstract class BaseGridMachine : MonoBehaviour, IItemHolder, IPowerConsumer
{
    protected WalkerGrid _grid;
    protected ConstructibleBuilding _constructible;

    protected float GridStepSize => _grid != null ? _grid.CellSize : 2f;

    // --- IPowerConsumer ---
    // PowerDemand kommt aus der BuildingDefinition — keine Überschreibung nötig
    public float PowerDemand => _constructible?.Definition?.PowerDemand ?? 0f;
    public bool IsPowered { get; set; } = false;

    // Maschinen ohne PowerDemand laufen immer
    protected bool CanOperate => !UsesPower || IsPowered;
    private bool UsesPower => (_constructible?.Definition?.UsesPower ?? false);

    protected virtual void Start()
    {
        _grid = GetComponentInParent<WalkerGrid>();
        if (_grid == null)
        {
            Debug.LogError($"'{gameObject.name}': Kein WalkerGrid im Parent gefunden!");
            return;
        }

        _constructible = GetComponent<ConstructibleBuilding>();

        Vector2Int gridSize = _constructible?.Definition?.GridSize ?? Vector2Int.one;
        int rotation = _constructible?.Rotation ?? 0;

        _grid.RegisterObject(transform.position, gridSize, rotation, this);

        // Beim PowerGrid registrieren falls nötig
        // PowerPole übernimmt das normalerweise, aber als Fallback:
        // Maschinen ohne PowerDemand brauchen keine Registrierung
        if (UsesPower)
        {
            PowerGrid.Instance?.RegisterConsumer(this);
            PowerPole.NotifyNewBuilding();
        }
    }

    protected virtual void OnDestroy()
    {
        if (_grid == null) return;

        Vector2Int gridSize = _constructible?.Definition?.GridSize ?? Vector2Int.one;
        int rotation = _constructible?.Rotation ?? 0;

        _grid.UnregisterObject(transform.position, gridSize, rotation);
        _grid.FreeCell(transform.position, gridSize, rotation);

        if (UsesPower)
            PowerGrid.Instance?.UnregisterConsumer(this);
    }

    // --- Port-API ---

    protected Vector3 GetPortWorldPosition(BuildingPort port)
    {
        int rotation = _constructible?.Rotation ?? 0;
        Vector2Int gridSize = _constructible?.Definition?.GridSize ?? Vector2Int.one;

        Vector2Int rotatedDir = port.GetRotatedDirection(rotation);
        Vector2Int rotatedOffset = port.GetRotatedEdgeOffset(rotation);

        int halfExtent = (rotatedDir.x != 0)
            ? (gridSize.x - 1) / 2
            : (gridSize.y - 1) / 2;

        Vector2Int portCell = rotatedDir * (halfExtent + 1) + rotatedOffset;
        Vector3 worldOffset = new Vector3(portCell.x, 0, portCell.y) * GridStepSize;
        return transform.position + _grid.transform.TransformDirection(worldOffset);
    }

    protected IItemHolder GetHolderAtPort(BuildingPort port)
    {
        if (_grid == null) return null;
        return _grid.GetHolderAt(GetPortWorldPosition(port));
    }

    protected bool IsPositionAtPort(Vector3 worldPos, BuildingPort port)
        => Vector3.Distance(worldPos, GetPortWorldPosition(port)) < GridStepSize * 0.75f;

    protected bool IsFromAnyInputPort(Vector3 worldPos)
    {
        if (_constructible?.Definition == null) return true;
        foreach (var port in _constructible.Definition.GetInputPorts())
            if (IsPositionAtPort(worldPos, port)) return true;
        return false;
    }

    protected bool TryPushItemToPort(BuildingPort port, ItemDefinition item, GameObject visualObj = null)
    {
        IItemHolder nextHolder = GetHolderAtPort(port);
        if (nextHolder == null || nextHolder.IsFull) return false;
        return nextHolder.TryAcceptItem(item, visualObj);
    }

    // --- Abstract ---

    public abstract bool TryAcceptItem(ItemDefinition item, GameObject visualObj = null);
    public abstract bool TryTakeItem(WorldItem item);
    public abstract bool IsFull { get; }
}