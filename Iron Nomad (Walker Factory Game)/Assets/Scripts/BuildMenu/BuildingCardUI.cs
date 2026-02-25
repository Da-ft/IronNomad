using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingCardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private Image _icon;
    [SerializeField] private Button _button;

    private BuildingDefinition _building;
    private System.Action<BuildingDefinition> _onSelected;

    public void Setup(BuildingDefinition building, System.Action<BuildingDefinition> onSelected)
    {
        _building = building;
        _onSelected = onSelected;
        _label.text = building.DisplayName;

        if (building.Icon != null)
            _icon.sprite = building.Icon;

        _button.onClick.AddListener(() => _onSelected?.Invoke(_building));
    }
}