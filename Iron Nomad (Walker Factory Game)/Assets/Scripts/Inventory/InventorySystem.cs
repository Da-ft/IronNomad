using UnityEngine;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    [Header("Config")]
    public int TotalSlots => _totalSlots;

    [SerializeField] private int _totalSlots = 36;

    [Header("Debug View")]
    [SerializeField] private List<InventorySlot> _slots = new List<InventorySlot>();

    public event System.Action OnInventoryChanged;

    private void Awake()
    {
        for (int i = 0; i < _totalSlots; i++)
            _slots.Add(new InventorySlot());
    }

    public InventorySlot GetSlotAt(int index)
    {
        if (index < 0 || index >= _slots.Count) return null;
        return _slots[index];
    }

    public bool AddItem(ItemDefinition item, int amount = 1)
    {
        // Erst in existierende Stacks auffüllen
        foreach (var slot in _slots)
        {
            if (!slot.IsEmpty && slot.Item == item && slot.Count < item.MaxStackSize)
            {
                int toAdd = Mathf.Min(item.MaxStackSize - slot.Count, amount);
                slot.Count += toAdd;
                amount -= toAdd;

                if (amount <= 0)
                {
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        // Dann leere Slots suchen
        while (amount > 0)
        {
            InventorySlot emptySlot = null;
            foreach (var slot in _slots)
            {
                if (slot.IsEmpty) { emptySlot = slot; break; }
            }

            if (emptySlot == null)
            {
                Debug.Log("Inventar voll!");
                return false;
            }

            int toAdd = Mathf.Min(item.MaxStackSize, amount);
            emptySlot.Add(item, toAdd);
            amount -= toAdd;
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(ItemDefinition item, int amount = 1)
    {
        int totalCount = 0;
        foreach (var slot in _slots)
            if (!slot.IsEmpty && slot.Item == item) totalCount += slot.Count;

        if (totalCount < amount) return false;

        foreach (var slot in _slots)
        {
            if (!slot.IsEmpty && slot.Item == item)
            {
                int toTake = Mathf.Min(slot.Count, amount);
                slot.Count -= toTake;
                amount -= toTake;

                if (slot.Count <= 0) slot.Clear();
                if (amount <= 0) break;
            }
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public void SwapSlots(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= _slots.Count) return;
        if (indexB < 0 || indexB >= _slots.Count) return;

        InventorySlot slotA = _slots[indexA];
        InventorySlot slotB = _slots[indexB];

        // Gleicher Typ: zusammenführen
        if (!slotA.IsEmpty && !slotB.IsEmpty && slotA.Item == slotB.Item)
        {
            int maxStack = slotB.Item.MaxStackSize;
            int space = maxStack - slotB.Count;
            int transfer = Mathf.Min(space, slotA.Count);

            slotB.Count += transfer;
            slotA.Count -= transfer;

            if (slotA.Count <= 0)
                slotA.Clear();

            OnInventoryChanged?.Invoke();
            return;
        }

        // Unterschiedliche Items: tauschen
        ItemDefinition tempItem = slotA.Item;
        int tempCount = slotA.Count;

        slotA.Item = slotB.Item;
        slotA.Count = slotB.Count;

        slotB.Item = tempItem;
        slotB.Count = tempCount;

        OnInventoryChanged?.Invoke();
    }

    public void SortByType()
    {
        MergeStacks();

        _slots.Sort((a, b) =>
        {
            if (a.IsEmpty && b.IsEmpty) return 0;
            if (a.IsEmpty) return 1;
            if (b.IsEmpty) return -1;
            return string.Compare(a.Item.Name, b.Item.Name);
        });

        OnInventoryChanged?.Invoke();
    }

    private void MergeStacks()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i].IsEmpty) continue;

            int maxStack = _slots[i].Item.MaxStackSize;

            for (int j = i + 1; j < _slots.Count; j++)
            {
                if (_slots[j].IsEmpty) continue;
                if (_slots[j].Item != _slots[i].Item) continue;

                int space = maxStack - _slots[i].Count;
                if (space <= 0) break;

                int transfer = Mathf.Min(space, _slots[j].Count);
                _slots[i].Count += transfer;
                _slots[j].Count -= transfer;

                if (_slots[j].Count <= 0)
                    _slots[j].Clear();
            }
        }
    }

    public bool TakeFromStack(int slotIndex, int amount)
    {
        InventorySlot slot = GetSlotAt(slotIndex);
        if (slot == null || slot.IsEmpty || slot.Count < amount) return false;

        slot.Count -= amount;
        if (slot.Count <= 0) slot.Clear();

        OnInventoryChanged?.Invoke();
        return true;
    }

    public void SplitStack(int slotIndex)
    {
        InventorySlot slot = GetSlotAt(slotIndex);
        if (slot == null || slot.IsEmpty || slot.Count < 2) return;

        int half = slot.Count / 2;
        int remainder = slot.Count - half;

        for (int i = 0; i < _slots.Count; i++)
        {
            if (i == slotIndex) continue;
            if (_slots[i].IsEmpty)
            {
                _slots[i].Add(slot.Item, half);
                slot.Count = remainder;
                OnInventoryChanged?.Invoke();
                return;
            }
        }

        Debug.Log("Kein freier Slot zum Teilen!");
    }

    public void ExpandInventory(int extraSlots)
    {
        for (int i = 0; i < extraSlots; i++)
            _slots.Add(new InventorySlot());
        _totalSlots += extraSlots;
        OnInventoryChanged?.Invoke();
    }

    public int GetItemCount(ItemDefinition item)
    {
        int total = 0;
        foreach (var slot in _slots)
            if (!slot.IsEmpty && slot.Item == item) total += slot.Count;
        return total;
    }

    public void NotifyChanged() => OnInventoryChanged?.Invoke();
}