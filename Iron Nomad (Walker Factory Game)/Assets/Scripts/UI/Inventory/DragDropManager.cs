using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DragDropManager : MonoBehaviour
{
    public static DragDropManager Instance { get; private set; }

    [SerializeField] private Image _dragIcon;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TextMeshProUGUI _dragCountText;

    public int DraggedSlotIndex { get; private set; } = -1;
    public bool IsDragging => DraggedSlotIndex >= 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _dragIcon.raycastTarget = false;
        _dragIcon.gameObject.SetActive(false);
    }

    public void BeginDrag(int slotIndex, Sprite icon, int count)
    {
        DraggedSlotIndex = slotIndex;
        _dragIcon.sprite = icon;
        _dragIcon.color = icon != null ? Color.white : Color.gray;
        _dragCountText.text = count > 1 ? count.ToString() : "";
        _dragIcon.gameObject.SetActive(true);
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
        _dragIcon.gameObject.SetActive(false);
    }
}