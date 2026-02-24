using UnityEngine;

public abstract class BaseGridMachine : MonoBehaviour, IItemHolder
{
    protected WalkerGrid _grid;
    protected float GridStepSize => _grid != null ? _grid.CellSize : 2f;

    protected virtual void Start()
    {
        _grid = GetComponentInParent<WalkerGrid>();

        if (_grid == null)
        {
            Debug.LogError($"'{gameObject.name}': Kein WalkerGrid im Parent gefunden!");
            return;
        }

        _grid.RegisterObject(transform.position, this);
    }

    protected virtual void OnDestroy()
    {
        if (_grid != null)
        {
            _grid.UnregisterObject(transform.position);
        }
    }

    public abstract bool TryAcceptItem(ItemDefinition item, GameObject visualObj = null);
    public abstract bool TryTakeItem(WorldItem item);
}
