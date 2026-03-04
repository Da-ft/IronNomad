using UnityEngine;

public interface IItemHolder
{
    bool TryAcceptItem(ItemDefinition item, GameObject visualObj = null);
    bool TryTakeItem(WorldItem item);
    bool IsFull { get; }
}