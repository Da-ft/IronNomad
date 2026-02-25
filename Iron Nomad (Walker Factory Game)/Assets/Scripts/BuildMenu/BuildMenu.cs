using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IronNomad.Inputs;

public class BuildMenu : MonoBehaviour, IMenu
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private BuilderTool _builderTool;

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

    private void OnEnable() => _inputReader.BuildModeEvent += ToggleMenu;
    private void OnDisable() => _inputReader.BuildModeEvent -= ToggleMenu;

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

    // --- IMenu ---

    public void Open()
    {
        _isOpen = true;
        _menuRoot.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _inputReader.DisableGameplay();
        _inputReader.ResetLook();
        _inputReader.ResetMove();
    }

    public void Close()
    {
        _isOpen = false;
        _menuRoot.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _inputReader.EnableGameplay();
    }

    private void ToggleMenu()
    {
        if (_isOpen)
            Close();
        else
            UIManager.Instance.OpenMenu(this);
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
        Close();
        _builderTool.SelectBuilding(building);
        StartCoroutine(EnableBuildModeNextFrame());
    }

    private System.Collections.IEnumerator EnableBuildModeNextFrame()
    {
        yield return null;
        _builderTool.EnableBuildMode();
    }
}