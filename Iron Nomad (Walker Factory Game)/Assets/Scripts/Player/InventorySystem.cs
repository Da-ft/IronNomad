using UnityEngine;
using IronNomad.Inputs;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;

    [Header("Config")]
    public int TotalSlots => _totalSlots;

    [SerializeField] private int _totalSlots = 36;
    [SerializeField] private int _maxStackSize = 64;

    [Header("Debug View")]
    [SerializeField] private List<InventorySlot> _slots = new List<InventorySlot>();
    [SerializeField] private int _selectedHotbarIndex = 0;

    public event System.Action OnInventoryChanged;

    private void Awake()
    {
        for (int i = 0; i < _totalSlots; i++)
        {
            _slots.Add(new InventorySlot());
        }
    }

    private void OnEnable()
    {
        if (_inputReader == null) return;
    }

    private void OnDisable()
    {
        if (_inputReader == null) return;
    }

    public InventorySlot GetSlotAt(int index)
    {
        if (index < 0 || index >= _slots.Count) return null;
        return _slots[index];
    }

    // --- Public API ---
    // Gibt aktuell ausgewähltes Item zurück (für BuilderTool etc.)
    public InventorySlot GetSelectedSlot()
    {
        return _slots[_selectedHotbarIndex];
    }

    public ItemDefinition GetSelectedItem()
    {
        var slot = GetSelectedSlot();
        return slot.IsEmpty ? null : slot.Item;
    }

    // Item hinzufügen
    // Gibt "true" zurück, wenn alles aufgenommen wurde, und "false" wenn Inventar voll ist
    public bool AddItem(ItemDefinition item, int amount = 1)
    {
        foreach (var slot in _slots)
        {
            if (!slot.IsEmpty && slot.Item == item)
            {
                if (slot.Count < _maxStackSize)
                {
                    int space = _maxStackSize - slot.Count;
                    int toAdd = Mathf.Min(space, amount);

                    slot.Count += toAdd;
                    amount -= toAdd;

                    if (amount <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
        }

        // 2. Versuch stuff in den leeren Slot zu packen
        foreach (var slot in _slots)
        {
            if (slot.IsEmpty)
            {
                slot.Add(item, amount);
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        // Inventar voll
        Debug.Log("Inventur voll!");
        OnInventoryChanged?.Invoke();
        return false;
    }

    public bool RemoveItem(ItemDefinition item, int amount = 1)
    {
        int totalCount = 0;
        foreach (var slot in _slots)
        {
            if (!slot.IsEmpty && slot.Item == item) totalCount += slot.Count;
        }

        if (totalCount < amount) return false;

        // Entfernen
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

    public void ExpandInventory(int extraSlots)
    {
        for (int i = 0; i < extraSlots; i++)
        {
            _slots.Add(new InventorySlot());
        }
        _totalSlots += extraSlots;
        OnInventoryChanged?.Invoke();
    }
}
