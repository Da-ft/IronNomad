using UnityEngine;

public interface IItemHolder
{
    bool TryTakeItem(WorldItem item);

    bool TryAcceptItem(ItemDefinition item, GameObject visualObj = null);
}
