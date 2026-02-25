using System.Collections.Generic;
using UnityEngine;

public class HotbarUI : MonoBehaviour
{
    [SerializeField] private InventorySystem _inventory;
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private Transform _slotContainer;

    private List<HotbarSlotUI> _slots = new List<HotbarSlotUI>();
    private int _selectedIndex = 0;

    private void Start()
    {
        BuildSlots();
        Refresh();
    }

    private void OnEnable()
    {
        _inventory.OnInventoryChanged += Refresh;
        _inventory.OnSelectionChanged += OnSelectionChanged;
    }

    private void OnDisable()
    {
        _inventory.OnInventoryChanged -= Refresh;
        _inventory.OnSelectionChanged -= OnSelectionChanged;
    }

    private void BuildSlots()
    {
        // Hotbar Größe aus InventorySystem holen
        for (int i = 0; i < _inventory.HotbarSize; i++)
        {
            GameObject obj = Instantiate(_slotPrefab, _slotContainer);
            HotbarSlotUI slot = obj.GetComponent<HotbarSlotUI>();
            _slots.Add(slot);
        }
    }

    private void Refresh()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            InventorySlot data = _inventory.GetSlotAt(i);
            _slots[i].UpdateSlot(data, i == _selectedIndex);
        }
    }

    private void OnSelectionChanged(int index)
    {
        _selectedIndex = index;
        Refresh();
    }
}