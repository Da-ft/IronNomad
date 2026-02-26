using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrashSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private InventorySystem _inventory;
    [SerializeField] private Image _highlight;

    public void OnDrop(PointerEventData eventData)
    {
        if (!DragDropManager.Instance.IsDragging) return;

        int fromIndex = DragDropManager.Instance.DraggedSlotIndex;
        InventorySlot slot = _inventory.GetSlotAt(fromIndex);

        if (slot == null || slot.IsEmpty) return;

        Debug.Log($"{slot.Item.Name} x{slot.Count} gelöscht!");
        slot.Clear();
        _inventory.NotifyChanged();
    }
}