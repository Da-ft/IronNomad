using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IronNomad.Inputs;

public class BuildMenu : MonoBehaviour, IMenu
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;

    [Header("UI References")]
    [SerializeField] private GameObject _menuRoot;
    [SerializeField] private Transform _categoryList;
    [SerializeField] private Transform _buildingGrid;

    [Header("Prefabs")]
    [SerializeField] private GameObject _categoryButtonPrefab;
    [SerializeField] private GameObject _buildingCardPrefab;

    [Header("Gebäude")]
    [SerializeField] private List<BuildingDefinition> _allBuildings;

    public bool IsOpen => _isOpen;
    private bool _isOpen = false;
    private bool _isEnabled = false;
    private BuilderTool _builderTool;

    private void Awake()
    {
        _allBuildings = new List<BuildingDefinition>(Resources.LoadAll<BuildingDefinition>("Buildings"));
    }

    private void Start()
    {
        UIManager.Instance.RegisterMenu(this);
        _menuRoot.SetActive(false);
        BuildCategoryList();
    }

    private void OnDestroy()
    {
        UIManager.Instance?.UnregisterMenu(this);
    }

    public void SetEnabled(bool enabled, BuilderTool tool)
    {
        _isEnabled = enabled;
        _builderTool = enabled ? tool : null;

        if (enabled)
            _inputReader.BuildModeEvent += ToggleMenu;
        else
            _inputReader.BuildModeEvent -= ToggleMenu;
    }

    // --- IMenu ---

    public void Open()
    {
        if (!_isEnabled) return;
        _isOpen = true;
        _menuRoot.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _inputReader.DisableGameplay();
        _inputReader.ResetLook();
        _inputReader.ResetMove();
    }

    // Close macht nur das UI zu
    public void Close()
    {
        _isOpen = false;
        _menuRoot.SetActive(false);
    }

    private void ToggleMenu()
    {
        if (_isOpen) UIManager.Instance.CloseAll();
        else UIManager.Instance.OpenMenu(this);
    }

    // --- Category & Building Lists ---

    private void BuildCategoryList()
    {
        var categories = _allBuildings
            .Where(b => b.Category != null)
            .Select(b => b.Category)
            .Distinct()
            .ToList();

        foreach (var category in categories)
        {
            GameObject obj = Instantiate(_categoryButtonPrefab, _categoryList);
            CategoryButtonUI btn = obj.GetComponent<CategoryButtonUI>();
            btn.Setup(category, OnCategorySelected);
        }

        if (categories.Count > 0)
            OnCategorySelected(categories[0]);
    }

    private void OnCategorySelected(BuildingCategory category)
    {
        foreach (Transform child in _buildingGrid)
            Destroy(child.gameObject);

        var buildings = _allBuildings
            .Where(b => b.Category == category && b.IsUnlockedByDefault)
            .ToList();

        foreach (var building in buildings)
        {
            GameObject obj = Instantiate(_buildingCardPrefab, _buildingGrid);
            BuildingCardUI card = obj.GetComponent<BuildingCardUI>();
            card.Setup(building, OnBuildingSelected);
        }
    }

    private void OnBuildingSelected(BuildingDefinition building)
    {
        UIManager.Instance.CloseAll();
        _builderTool?.SelectBuilding(building);
    }
}