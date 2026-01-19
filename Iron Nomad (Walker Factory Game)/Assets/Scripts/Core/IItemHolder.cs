public interface IItemHolder
{
    // Call wenn der Spieler das item nimmt
    // Rückabe: Is he allowed tho?
    bool TryTakeItem(WorldItem item);
}
