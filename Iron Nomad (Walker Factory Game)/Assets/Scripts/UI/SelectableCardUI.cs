using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Generischer auswählbarer Button mit Icon, Label und optionalem Hover-Callback.
/// Wird von BuildMenu, MachineMenuUI, WalkerPowerMenuUI etc. verwendet.
/// </summary>
public class SelectableCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private Image _icon;
    [SerializeField] private Image _background;
    [SerializeField] private Button _button;

    [Header("Colors")]
    [SerializeField] private Color _colorNormal = Color.white;
    [SerializeField] private Color _colorSelected = new Color(0.3f, 0.8f, 0.3f, 1f);
    [SerializeField] private Color _colorHover = new Color(1f, 1f, 1f, 0.8f);

    private System.Action _onClick;
    private System.Action _onHoverEnter;
    private System.Action _onHoverExit;

    public void Setup(
        string label,
        Sprite icon,
        System.Action onClick,
        System.Action onHoverEnter = null,
        System.Action onHoverExit = null)
    {
        if (_label != null) _label.text = label;
        if (_icon != null)
        {
            _icon.sprite = icon;
            _icon.enabled = icon != null;
        }

        _onClick = onClick;
        _onHoverEnter = onHoverEnter;
        _onHoverExit = onHoverExit;

        _button.onClick.AddListener(() => _onClick?.Invoke());
        SetColor(_colorNormal);
    }

    public void SetSelected(bool selected)
        => SetColor(selected ? _colorSelected : _colorNormal);

    public void OnPointerEnter(PointerEventData _)
    {
        SetColor(_colorHover);
        _onHoverEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData _)
    {
        SetColor(_colorNormal);
        _onHoverExit?.Invoke();
    }

    private void SetColor(Color c)
    {
        if (_background != null) _background.color = c;
    }
}