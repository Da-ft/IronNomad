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

        // Nur noch für Item-Pipeline registrieren, keine Zellen-Belegung mehr
        _grid.RegisterObject(transform.position, this);
    }

    protected virtual void OnDestroy()
    {
        if (_grid != null)
        {
            _grid.UnregisterObject(transform.position);
            _grid.FreeCell(transform.position); // Zelle freigeben
        }
    }

    public abstract bool TryAcceptItem(ItemDefinition item, GameObject visualObj = null);
    public abstract bool TryTakeItem(WorldItem item);
}
