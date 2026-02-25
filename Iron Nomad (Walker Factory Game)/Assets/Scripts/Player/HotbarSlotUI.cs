using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarSlotUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _count;
    [SerializeField] private Image _selectionHighlight;

    public void UpdateSlot(InventorySlot slot, bool isSelected)
    {
        // Icon
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

        // Count
        _count.text = !slot.IsEmpty && slot.Count > 1 ? slot.Count.ToString() : "";

        // Selection Highlight
        _selectionHighlight.enabled = isSelected;
    }
}