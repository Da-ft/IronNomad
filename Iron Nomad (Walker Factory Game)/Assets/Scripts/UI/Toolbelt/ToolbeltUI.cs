using System.Collections.Generic;
using UnityEngine;

// Ehemals HotbarUI – zeigt die aktiven Tools des Spielers
public class ToolbeltUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ToolbeltSystem _toolbelt;

    [Header("UI")]
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private Transform _slotContainer;

    private List<SlotUI> _slots = new List<SlotUI>();

    private void OnEnable()
    {
        _toolbelt.OnSelectionChanged += Refresh;
    }

    private void OnDisable()
    {
        _toolbelt.OnSelectionChanged -= Refresh;
    }

    private void Start()
    {
        BuildSlots();
        Refresh(_toolbelt.SelectedIndex);
    }

    private void BuildSlots()
    {
        for (int i = 0; i < _toolbelt.ToolCount; i++)
        {
            GameObject obj = Instantiate(_slotPrefab, _slotContainer);
            _slots.Add(obj.GetComponent<SlotUI>());
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