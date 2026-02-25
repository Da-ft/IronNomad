using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CategoryButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private Button _button;

    private BuildingCategory _category;
    private System.Action<BuildingCategory> _onSelected;

    public void Setup(BuildingCategory category, System.Action<BuildingCategory> onSelected)
    {
        _category = category;
        _onSelected = onSelected;
        _label.text = category.DisplayName;
        _button.onClick.AddListener(() => _onSelected?.Invoke(_category));
    }
}