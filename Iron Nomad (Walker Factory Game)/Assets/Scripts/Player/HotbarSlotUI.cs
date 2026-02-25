using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarSlotUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _count;
    [SerializeField] private Image _selectionHighlight;

    // Für Inventar-Slots (Items)
    public void UpdateSlot(InventorySlot slot, bool isSelected)
    {
        if (!slot.IsEmpty && slot.Item.Icon != null)
        {
            _icon.sprite = slot.Item.Icon;
            _icon.enabled = true;
        }
        else
        {
            _icon.sprite = null;
            _icon.enabled = false;
        }

        _count.text = !slot.IsEmpty && slot.Count > 1 ? slot.Count.ToString() : "";
        _selectionHighlight.enabled = isSelected;
    }

    // Für Toolbelt-Slots (Werkzeuge)
    public void UpdateToolSlot(ToolDefinition tool, bool isSelected)
    {
        if (tool != null && tool.Icon != null)
        {
            _icon.sprite = tool.Icon;
            _icon.enabled = true;
        }
        else
        {
            _icon.sprite = null;
            _icon.enabled = false;
        }

        _count.text = "";
        _selectionHighlight.enabled = isSelected;
    }

    public void SetEmpty(bool isSelected)
    {
        _icon.sprite = null;
        _icon.enabled = false;
        _count.text = "";
        _selectionHighlight.enabled = isSelected;
    }
}