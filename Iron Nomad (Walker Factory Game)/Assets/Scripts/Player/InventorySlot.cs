using System;

[Serializable]
public class InventorySlot
{
    public ItemDefinition Item;
    public int Count;

    public bool IsEmpty => Item == null;

    public void Clear()
    {
        Item = null;
        Count = 0;
    }

    public void Add(ItemDefinition item, int amount)
    {
        Item = item;
        Count += amount;
    }
}
