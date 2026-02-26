using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class HotbarSlotUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
    IPointerClickHandler
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _count;
    [SerializeField] private Image _selectionHighlight;

    public int SlotIndex { get; private set; }
    private InventorySystem _inventory;

    public void Setup(int slotIndex, InventorySystem inventory)
    {
        SlotIndex = slotIndex;
        _inventory = inventory;
    }

    // --- Display ---

    public void UpdateSlot(InventorySlot slot, bool isSelected)
    {
        if (!slot.IsEmpty)
        {
            _icon.enabled = true;
            _icon.sprite = slot.Item.Icon;
            _icon.color = slot.Item.Icon != null ? Color.white : Color.gray;
        }
        else
        {
            _icon.enabled = false;
            _icon.sprite = null;
            _icon.color = Color.white;
        }

        _count.text = !slot.IsEmpty && slot.Count > 1 ? slot.Count.ToString() : "";
        _selectionHighlight.enabled = isSelected;
    }

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

    // --- Drag & Drop ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        InventorySlot slot = _inventory.GetSlotAt(SlotIndex);
        if (slot == null || slot.IsEmpty) return;

        DragDropManager.Instance.BeginDrag(SlotIndex, slot.Item.Icon);
        _icon.enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragDropManager.Instance.UpdateDragPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragDropManager.Instance.EndDrag();
        InventorySlot slot = _inventory.GetSlotAt(SlotIndex);
        if (slot != null && !slot.IsEmpty)
            _icon.enabled = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!DragDropManager.Instance.IsDragging) return;

        int fromIndex = DragDropManager.Instance.DraggedSlotIndex;
        if (fromIndex == SlotIndex) return;

        _inventory.SwapSlots(fromIndex, SlotIndex);
    }

    // --- Right Click: Halbieren ---

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_inventory == null) return;
        if (eventData.button != PointerEventData.InputButton.Right) return;

        InventorySlot slot = _inventory.GetSlotAt(SlotIndex);
        if (slot == null || slot.IsEmpty) return;

        _inventory.SplitStack(SlotIndex);
    }
}