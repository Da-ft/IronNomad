using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private Image _icon;
    [SerializeField] private Button _button;

    private BuildingDefinition _building;
    private System.Action<BuildingDefinition> _onSelected;
    private BuildMenu _buildMenu;

    public void Setup(
        BuildingDefinition building,
        System.Action<BuildingDefinition> onSelected,
        BuildMenu buildMenu)
    {
        _building = building;
        _onSelected = onSelected;
        _buildMenu = buildMenu;

        _label.text = building.DisplayName;
        if (building.Icon != null) _icon.sprite = building.Icon;

        _button.onClick.AddListener(() => _onSelected?.Invoke(_building));
    }

    public void OnPointerEnter(PointerEventData _) => _buildMenu?.SetHoveredBuilding(_building);
    public void OnPointerExit(PointerEventData _) => _buildMenu?.ClearHoveredBuilding(_building);
}