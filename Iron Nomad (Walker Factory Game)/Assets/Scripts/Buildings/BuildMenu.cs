using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildMenu : MonoBehaviour, IMenu
{
    [Header("UI References")]
    [SerializeField] private GameObject _menuRoot;
    [SerializeField] private Transform _categoryList;
    [SerializeField] private Transform _buildingGrid;

    [Header("Prefabs")]
    [SerializeField] private GameObject _categoryButtonPrefab;
    [SerializeField] private GameObject _buildingCardPrefab; // SelectableCardUI

    public event System.Action<BuildingDefinition> OnBuildingSelected;

    public bool IsOpen => _isOpen;
    private bool _isOpen = false;

    private BuilderTool _builderTool;
    private List<BuildingDefinition> _allBuildings;
    private BuildingDefinition _hoveredBuilding;

    private void Awake()
    {
        _allBuildings = new List<BuildingDefinition>(
            Resources.LoadAll<BuildingDefinition>("Buildings"));
    }

    private void Start()
    {
        UIManager.Instance.RegisterMenu(this);
        _menuRoot.SetActive(false);
        BuildCategoryList();
    }

    private void OnEnable() => InputEvents.OnHotbarSelect += AssignToHotbar;
    private void OnDisable() => InputEvents.OnHotbarSelect -= AssignToHotbar;
    private void OnDestroy() => UIManager.Instance?.UnregisterMenu(this);

    private void AssignToHotbar(int index)
    {
        if (!_isOpen || _hoveredBuilding == null) return;
        HotbarSystem.Instance?.AssignSlot(index, _hoveredBuilding);
        Debug.Log($"[BuildMenu] {_hoveredBuilding.DisplayName} → Slot {index + 1}");
    }

    public void SetBuilderTool(BuilderTool tool) => _builderTool = tool;

    public void Open() { _isOpen = true; _menuRoot.SetActive(true); }
    public void Close() { _isOpen = false; _menuRoot.SetActive(false); _hoveredBuilding = null; }

    private void BuildCategoryList()
    {
        var categories = _allBuildings
            .Where(b => b.Category != null)
            .Select(b => b.Category)
            .Distinct().ToList();

        foreach (var cat in categories)
        {
            GameObject obj = Instantiate(_categoryButtonPrefab, _categoryList);
            obj.GetComponent<CategoryButtonUI>().Setup(cat, OnCategorySelected);
        }

        if (categories.Count > 0) OnCategorySelected(categories[0]);
    }

    private void OnCategorySelected(BuildingCategory category)
    {
        foreach (Transform child in _buildingGrid) Destroy(child.gameObject);

        foreach (var building in _allBuildings.Where(b => b.Category == category && b.IsUnlockedByDefault))
        {
            var def = building; // capture
            GameObject obj = Instantiate(_buildingCardPrefab, _buildingGrid);
            var card = obj.GetComponent<SelectableCardUI>();
            card.Setup(
                label: def.DisplayName,
                icon: def.Icon,
                onClick: () => OnBuildingSelected?.Invoke(def),
                onHoverEnter: () => _hoveredBuilding = def,
                onHoverExit: () => { if (_hoveredBuilding == def) _hoveredBuilding = null; }
            );
        }
    }
}