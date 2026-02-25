using System.Collections.Generic;
using UnityEngine;

public class HotbarUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ToolbeltSystem _toolbelt;

    [Header("UI")]
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private Transform _slotContainer;

    private List<HotbarSlotUI> _slots = new List<HotbarSlotUI>();

    private void Start()
    {
        BuildSlots();
        Refresh(_toolbelt.SelectedIndex);
    }

    private void OnEnable()
    {
        _toolbelt.OnSelectionChanged += Refresh;
    }

    private void OnDisable()
    {
        _toolbelt.OnSelectionChanged -= Refresh;
    }

    private void BuildSlots()
    {
        for (int i = 0; i < _toolbelt.ToolCount; i++)
        {
            GameObject obj = Instantiate(_slotPrefab, _slotContainer);
            HotbarSlotUI slot = obj.GetComponent<HotbarSlotUI>();
            _slots.Add(slot);
        }
    }

    private void Refresh(int selectedIndex)
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            ToolDefinition tool = _toolbelt.GetToolAt(i);
            if (tool != null)
                _slots[i].UpdateToolSlot(tool, i == selectedIndex);
            else
                _slots[i].SetEmpty(i == selectedIndex);
        }
    }
}