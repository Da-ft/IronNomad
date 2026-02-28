using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class AtomizerSlotUI : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _itemName;

    private ItemDefinition _currentItem;
    private InventorySystem _inventory;

    public void Setup(InventorySystem inventory)
    {
        _inventory = inventory;
        ClearSlot();
    }

    public ItemDefinition GetItem() => _currentItem;

    public void ClearSlot()
    {
        _currentItem = null;
        _icon.sprite = null;
        _icon.enabled = false;
        if (_itemName != null) _itemName.text = "Item hier ablegen";
    }

    // Gibt das Item zurück ins Inventar (beim Schließen)
    public void ReturnItemToInventory()
    {
        if (_currentItem == null) return;
        _inventory?.AddItem(_currentItem, 1);
        ClearSlot();
    }

    // --- Drop ---

    public void OnDrop(PointerEventData eventData)
    {
        if (!DragDropManager.Instance.IsDragging) return;

        int fromIndex = DragDropManager.Instance.DraggedSlotIndex;
        InventorySlot sourceSlot = _inventory?.GetSlotAt(fromIndex);
        if (sourceSlot == null || sourceSlot.IsEmpty) return;

        // Altes Item zurück ins Inventar
        if (_currentItem != null)
            _inventory.AddItem(_currentItem, 1);

        // Genau 1 Item aus dem Stack nehmen
        _currentItem = sourceSlot.Item;
        _inventory.TakeFromStack(fromIndex, 1);

        // Anzeigen
        _icon.sprite = _currentItem.Icon;
        _icon.enabled = _currentItem.Icon != null;
        if (_itemName != null) _itemName.text = _currentItem.Name;
    }
}