using UnityEngine;

public abstract class BaseGridMachine : MonoBehaviour, IItemHolder
{
    protected WalkerGrid _grid;
    protected ConstructibleBuilding _constructible;

    protected float GridStepSize => _grid != null ? _grid.CellSize : 2f;

    protected virtual void Start()
    {
        _grid = GetComponentInParent<WalkerGrid>();
        if (_grid == null)
        {
            Debug.LogError($"'{gameObject.name}': Kein WalkerGrid im Parent gefunden!");
            return;
        }

        _constructible = GetComponent<ConstructibleBuilding>();

        // Grid-Registrierung
        Vector2Int gridSize = _constructible?.Definition?.GridSize ?? Vector2Int.one;
        int rotation = _constructible?.Rotation ?? 0;

        _grid.RegisterObject(transform.position, gridSize, rotation, this);
    }

    protected virtual void OnDestroy()
    {
        if (_grid == null) return;

        Vector2Int gridSize = _constructible?.Definition?.GridSize ?? Vector2Int.one;
        int rotation = _constructible?.Rotation ?? 0;

        _grid.UnregisterObject(transform.position, gridSize, rotation);
        _grid.FreeCell(transform.position, gridSize, rotation);
    }

    // --- Port-API ---

    /// <summary>
    /// Gibt die Weltposition eines Ports zurück.
    /// Berücksichtigt die Gebäudegröße (Port liegt am Rand, nicht an der Mitte)
    /// und den EdgeOffset (Verschiebung entlang der Kante).
    /// </summary>
    protected Vector3 GetPortWorldPosition(BuildingPort port)
    {
        int rotation = _constructible?.Rotation ?? 0;
        Vector2Int gridSize = _constructible?.Definition?.GridSize ?? Vector2Int.one;

        Vector2Int rotatedDir = port.GetRotatedDirection(rotation);
        Vector2Int rotatedOffset = port.GetRotatedEdgeOffset(rotation);

        // Wie viele Zellen vom Center bis zum Rand in Port-Richtung?
        // Bei GridSize (3,3) und Direction (0,1): 3/2 = 1 (abgerundet) → Rand bei +1, Port bei +2
        int halfExtent = (rotatedDir.x != 0)
            ? (gridSize.x - 1) / 2
            : (gridSize.y - 1) / 2;

        // Port = Center + Richtung*(halfExtent+1) + EdgeOffset
        Vector2Int portCell = rotatedDir * (halfExtent + 1) + rotatedOffset;

        Vector3 worldOffset = new Vector3(portCell.x, 0, portCell.y) * GridStepSize;
        return transform.position + _grid.transform.TransformDirection(worldOffset);
    }

    /// <summary>
    /// Gibt den IItemHolder zurück, der an einem bestimmten Port angeschlossen ist.
    /// </summary>
    protected IItemHolder GetHolderAtPort(BuildingPort port)
    {
        if (_grid == null) return null;
        return _grid.GetHolderAt(GetPortWorldPosition(port));
    }

    /// <summary>
    /// Prüft ob eine Weltposition zu einem bestimmten Input-Port gehört.
    /// Nützlich in TryAcceptItem um die Herkunft zu validieren.
    /// </summary>
    protected bool IsPositionAtPort(Vector3 worldPos, BuildingPort port)
    {
        return Vector3.Distance(worldPos, GetPortWorldPosition(port)) < GridStepSize * 0.75f;
    }

    /// <summary>
    /// Prüft ob eine Position zu irgendeinem Input-Port dieses Gebäudes gehört.
    /// </summary>
    protected bool IsFromAnyInputPort(Vector3 worldPos)
    {
        if (_constructible?.Definition == null) return true; // kein Check möglich → akzeptieren

        foreach (var port in _constructible.Definition.GetInputPorts())
        {
            if (IsPositionAtPort(worldPos, port))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Versucht, ein Item über einen Output-Port an den nächsten Holder weiterzugeben.
    /// Gibt true zurück wenn erfolgreich.
    /// </summary>
    protected bool TryPushItemToPort(BuildingPort port, ItemDefinition item, GameObject visualObj = null)
    {
        IItemHolder nextHolder = GetHolderAtPort(port);
        if (nextHolder == null || nextHolder.IsFull) return false;
        return nextHolder.TryAcceptItem(item, visualObj);
    }

    // --- Interface (von Subklassen implementiert) ---

    public abstract bool TryAcceptItem(ItemDefinition item, GameObject visualObj = null);
    public abstract bool TryTakeItem(WorldItem item);
    public abstract bool IsFull { get; }
}