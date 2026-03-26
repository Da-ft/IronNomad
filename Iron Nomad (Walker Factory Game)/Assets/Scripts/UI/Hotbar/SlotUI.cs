using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SlotUI : MonoBehaviour,
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

    public void SetEmpty(bool isSelected)
    {
        _icon.sprite = null;
        _icon.enabled = false;
        _count.text = "";
        _selectionHighlight.enabled = isSelected;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"BeginDrag auf Slot {SlotIndex}");

        if (eventData.button != PointerEventData.InputButton.Left) return;
        InventorySlot slot = _inventory?.GetSlotAt(SlotIndex);
        if (slot == null || slot.IsEmpty) return;
        DragDropManager.Instance.BeginDrag(SlotIndex, slot.Item.Icon, slot.Count);
        _icon.color = new Color(1f, 1f, 1f, 0.4f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragDropManager.Instance.UpdateDragPosition(eventData.position);
        Debug.Log($"Drag über: {eventData.pointerCurrentRaycast.gameObject?.name}");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Icon wiederherstellen - OnDrop hat zu diesem Punkt bereits gefeuert
        InventorySlot slot = _inventory?.GetSlotAt(SlotIndex);
        if (slot != null && !slot.IsEmpty)
        {
            _icon.enabled = true;
            _icon.color = Color.white;
        }
        else
        {
            _icon.enabled = false;
            _icon.color = Color.white;
        }

        // EndDrag NACH Icon-Update aufrufen
        DragDropManager.Instance.EndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {

        if (!DragDropManager.Instance.IsDragging)
        {
            return;
        }

        int fromIndex = DragDropManager.Instance.DraggedSlotIndex;

        if (fromIndex == SlotIndex) return;
        _inventory?.SwapSlots(fromIndex, SlotIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_inventory == null) return;
        if (eventData.button != PointerEventData.InputButton.Right) return;
        InventorySlot slot = _inventory.GetSlotAt(SlotIndex);
        if (slot == null || slot.IsEmpty) return;
        _inventory.SplitStack(SlotIndex);
    }
}