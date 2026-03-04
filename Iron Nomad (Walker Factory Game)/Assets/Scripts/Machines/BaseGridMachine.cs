using UnityEngine;

public abstract class BaseGridMachine : MonoBehaviour, IItemHolder
{
    protected WalkerGrid _grid;
    protected float GridStepSize => _grid != null ? _grid.CellSize : 2f;

    private Vector2Int _gridSize = Vector2Int.one;
    private int _rotation = 0;

    protected virtual void Start()
    {
        _grid = GetComponentInParent<WalkerGrid>();
        if (_grid == null)
        {
            Debug.LogError($"'{gameObject.name}': Kein WalkerGrid im Parent gefunden!");
            return;
        }

        // GridSize und Rotation vom ConstructibleBuilding holen
        ConstructibleBuilding constructible = GetComponent<ConstructibleBuilding>();
        if (constructible != null && constructible.Definition != null)
        {
            _gridSize = constructible.Definition.GridSize;
            _rotation = constructible.Rotation;
        }

        _grid.RegisterObject(transform.position, _gridSize, _rotation, this);
    }

    protected virtual void OnDestroy()
    {
        if (_grid != null)
        {
            _grid.UnregisterObject(transform.position, _gridSize, _rotation);
            _grid.FreeCell(transform.position, _gridSize, _rotation);
        }
    }

    public abstract bool TryAcceptItem(ItemDefinition item, GameObject visualObj = null);
    public abstract bool TryTakeItem(WorldItem item);
    public abstract bool IsFull { get; }


    protected bool IsFromInputDirection(Vector3 worldPos)
    {
        ConstructibleBuilding constructible = GetComponent<ConstructibleBuilding>();
        if (constructible?.Definition == null) return true; // kein Check möglich → akzeptieren

        foreach (var dir in constructible.Definition.InputDirections)
        {
            Vector3 targetPos = _grid.GetDirectionWorldPos(transform.position, dir, constructible.Rotation);
            if (Vector3.Distance(worldPos, targetPos) < _grid.CellSize * 0.75f)
                return true;
        }
        return false;
    }

    protected IItemHolder GetNextHolder(Vector2Int[] outputDirections, int rotation)
    {
        ConstructibleBuilding constructible = GetComponent<ConstructibleBuilding>();
        if (constructible?.Definition == null) return null;

        foreach (var dir in outputDirections)
        {
            Vector3 targetPos = _grid.GetDirectionWorldPos(transform.position, dir, rotation);
            IItemHolder holder = _grid.GetHolderAt(targetPos);
            if (holder != null) return holder;
        }
        return null;
    }

}