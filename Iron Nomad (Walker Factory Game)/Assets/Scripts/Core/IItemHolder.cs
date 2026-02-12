public interface IItemHolder
{
    bool TryTakeItem(WorldItem item);

    bool TryAcceptItem(ItemDefinition item);
}
