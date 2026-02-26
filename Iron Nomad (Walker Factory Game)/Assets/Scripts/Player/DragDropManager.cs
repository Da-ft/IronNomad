using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DragDropManager : MonoBehaviour
{
    public static DragDropManager Instance { get; private set; }

    [SerializeField] private Image _dragIcon;
    [SerializeField] private TextMeshProUGUI _dragAmountText;
    [SerializeField] private Canvas _canvas;

    // Normaler Slot-Drag
    public int DraggedSlotIndex { get; private set; } = -1;

    // Split Drag
    public ItemDefinition DraggedItem { get; private set; }
    public int DraggedAmount { get; private set; }

    // True wenn irgendetwas gedraggt wird
    public bool IsDragging => DraggedSlotIndex >= 0 || DraggedItem != null;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _dragIcon.gameObject.SetActive(false);
    }

    // Normaler Drag von Slot zu Slot
    public void BeginDrag(int slotIndex, Sprite icon)
    {
        DraggedSlotIndex = slotIndex;
        DraggedItem = null;
        DraggedAmount = 0;

        _dragIcon.sprite = icon;
        _dragIcon.color = icon != null ? Color.white : Color.gray;
        if (_dragAmountText != null) _dragAmountText.text = "";
        _dragIcon.gameObject.SetActive(true);
    }

    // Split Drag - Item ohne Slot-Ursprung
    public void BeginDragWithItem(ItemDefinition item, int amount)
    {
        DraggedSlotIndex = -1;
        DraggedItem = item;
        DraggedAmount = amount;

        _dragIcon.sprite = item.Icon;
        _dragIcon.color = item.Icon != null ? Color.white : Color.gray;
        if (_dragAmountText != null) _dragAmountText.text = amount > 1 ? amount.ToString() : "";
        _dragIcon.gameObject.SetActive(true);
    }

    // Menge live updaten während Split Drag
    public void UpdateDragAmount(int amount)
    {
        DraggedAmount = amount;
        if (_dragAmountText != null)
            _dragAmountText.text = amount > 1 ? amount.ToString() : "";
    }

    public void UpdateDragPosition(Vector2 screenPos)
    {
        if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            _dragIcon.rectTransform.position = screenPos;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                screenPos,
                _canvas.worldCamera,
                out Vector2 localPoint
            );
            _dragIcon.rectTransform.localPosition = localPoint;
        }
    }

    public void EndDrag()
    {
        DraggedSlotIndex = -1;
        DraggedItem = null;
        DraggedAmount = 0;
        _dragIcon.gameObject.SetActive(false);
    }
}