using UnityEngine;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    [Header("Config")]
    public int TotalSlots => _totalSlots;

    [SerializeField] private int _totalSlots = 36;
    [SerializeField] private int _maxStackSize = 64;

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
            if (!slot.IsEmpty && slot.Item == item && slot.Count < _maxStackSize)
            {
                int toAdd = Mathf.Min(_maxStackSize - slot.Count, amount);
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
        foreach (var slot in _slots)
        {
            if (slot.IsEmpty)
            {
                slot.Add(item, amount);
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        Debug.Log("Inventar voll!");
        return false;
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

        InventorySlot temp = new InventorySlot();
        temp.Add(_slots[indexA].Item, _slots[indexA].Count);

        if (_slots[indexB].IsEmpty)
            _slots[indexA].Clear();
        else
        {
            _slots[indexA].Clear();
            _slots[indexA].Add(_slots[indexB].Item, _slots[indexB].Count);
        }

        _slots[indexB].Clear();
        if (!temp.IsEmpty)
            _slots[indexB].Add(temp.Item, temp.Count);

        OnInventoryChanged?.Invoke();
    }

    public void SortByType()
    {
        _slots.Sort((a, b) =>
        {
            if (a.IsEmpty && b.IsEmpty) return 0;
            if (a.IsEmpty) return 1;
            if (b.IsEmpty) return -1;
            return string.Compare(a.Item.Name, b.Item.Name);
        });

        OnInventoryChanged?.Invoke();
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

    public void NotifyChanged() => OnInventoryChanged?.Invoke();
}