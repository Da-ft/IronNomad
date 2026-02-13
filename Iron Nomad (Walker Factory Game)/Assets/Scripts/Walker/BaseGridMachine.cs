using UnityEngine;

public abstract class BaseGridMachine : MonoBehaviour, IItemHolder
{
    protected WalkerGrid _grid;

    // Computed Property: Holt sich die Größe direkt vom Grid
    protected float GridStepSize => _grid != null ? _grid.CellSize : 2f;

    protected virtual void Start()
    {
        _grid = GetComponentInParent<WalkerGrid>();
    }

    // Zwingt die Kinder, diese Methoden zu haben (Interface IItemHolder)
    public abstract bool TryAcceptItem(ItemDefinition item, GameObject visualObj = null);
    public abstract bool TryTakeItem(WorldItem item);
}
