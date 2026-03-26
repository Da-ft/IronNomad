using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Koordiniert BuildingList, SelectedPanel und die drei Surfaces.
/// Einzige Klasse die Placement-Entscheidungen trifft.
/// </summary>
public class WalkerPowerMenuUI : MonoBehaviour
{
    [Header("Left Panel")]
    [SerializeField] private Transform _buildingListContainer;
    [SerializeField] private GameObject _buildingCardPrefab;
    [SerializeField] private WalkerSelectedPanelUI _selectedPanel;

    [Header("Right Panel")]
    [SerializeField] private TextMeshProUGUI _productionLabel;
    [SerializeField] private TextMeshProUGUI _demandLabel;
    [SerializeField] private TextMeshProUGUI _balanceLabel;
    [SerializeField] private Image _statusIcon;
    [SerializeField] private Color _colorSurplus = new Color(0.2f, 1f, 0.4f);
    [SerializeField] private Color _colorDeficit = new Color(1f, 0.3f, 0.2f);

    [Header("Surfaces")]
    [SerializeField] private List<WalkerSurfaceGridUI> _surfaces;

    // --- State ---
    private InventorySystem _inventory;
    private BuildingDefinition _selectedBuilding;
    private CellSelectionInfo _selectedCell;
    private SelectableCardUI _selectedCard;
    private bool _built = false;

    // --- Tab Lifecycle ---

    public void OnTabOpened()
    {
        if (!_built) { BuildBuildingList(); _built = true; }

        _inventory = FindAnyObjectByType<InventorySystem>();

        foreach (var s in _surfaces)
        {
            s.OnCellSelected += OnCellSelected;
            s.OnPlacementChanged += RefreshStats;
        }

        if (PowerGrid.Instance != null)
            PowerGrid.Instance.OnPowerStateChanged += RefreshStats;

        RefreshStats();
    }

    public void OnTabClosed()
    {
        foreach (var s in _surfaces)
        {
            s.OnCellSelected -= OnCellSelected;
            s.OnPlacementChanged -= RefreshStats;
        }

        if (PowerGrid.Instance != null)
            PowerGrid.Instance.OnPowerStateChanged -= RefreshStats;
    }

    // --- Building List ---

    private void BuildBuildingList()
    {
        foreach (Transform child in _buildingListContainer) Destroy(child.gameObject);

        foreach (var def in Resources.LoadAll<BuildingDefinition>("Buildings"))
        {
            if (!def.IsProducer) continue;
            var captured = def;
            var obj = Instantiate(_buildingCardPrefab, _buildingListContainer);
            var card = obj.GetComponent<SelectableCardUI>() ?? obj.GetComponentInChildren<SelectableCardUI>();
            card.Setup(
                label: $"{def.DisplayName}\n{def.PowerProduction:F0} MW",
                icon: def.Icon,
                onClick: () => OnBuildingCardClicked(captured, card)
            );
        }
    }

    private void OnBuildingCardClicked(BuildingDefinition def, SelectableCardUI card)
    {
        _selectedCard?.SetSelected(false);
        _selectedCard = card;
        _selectedCard.SetSelected(true);
        _selectedBuilding = def;

        // Zelle bereits ausgewählt und frei → Preview anzeigen
        if (_selectedCell != null && _selectedCell.State == CellSelectionInfo.CellState.Free)
            _selectedPanel.ShowBuildPreview(def, CanAfford(def.Costs));
    }

    // --- Cell Selection ---

    private void OnCellSelected(CellSelectionInfo info)
    {
        _selectedCell = info;

        switch (info.State)
        {
            case CellSelectionInfo.CellState.Free:
                if (_selectedBuilding != null)
                    _selectedPanel.ShowBuildPreview(_selectedBuilding, CanAfford(_selectedBuilding.Costs));
                else
                    _selectedPanel.ShowEmpty();
                break;

            case CellSelectionInfo.CellState.Occupied:
                _selectedPanel.ShowOccupied(info.Building);
                break;

            case CellSelectionInfo.CellState.Obstacle:
                _selectedPanel.ShowObstacle(info.Obstacle, CanAfford(info.Obstacle.RepairCosts));
                break;
        }
    }

    // --- Button Handlers ---

    private void OnEnable()
    {
        _selectedPanel.OnBuildClicked += TryBuild;
        _selectedPanel.OnDemolishClicked += TryDemolish;
        _selectedPanel.OnRepairClicked += TryRepair;
    }

    private void OnDisable()
    {
        _selectedPanel.OnBuildClicked -= TryBuild;
        _selectedPanel.OnDemolishClicked -= TryDemolish;
        _selectedPanel.OnRepairClicked -= TryRepair;
    }

    private void TryBuild()
    {
        if (_selectedCell == null || _selectedBuilding == null) return;
        if (_selectedCell.State != CellSelectionInfo.CellState.Free) return;
        if (!_selectedCell.Surface.CanPlace(_selectedCell.X, _selectedCell.Y, _selectedBuilding)) return;
        if (!CanAfford(_selectedBuilding.Costs)) return;

        foreach (var cost in _selectedBuilding.Costs)
            _inventory.RemoveItem(cost.Item, cost.Amount);

        _selectedCell.Surface.PlaceBuilding(_selectedCell.X, _selectedCell.Y, _selectedBuilding);
        _selectedPanel.ShowOccupied(_selectedBuilding);
        _selectedCell.State = CellSelectionInfo.CellState.Occupied;
        _selectedCell.Building = _selectedBuilding;
    }

    private void TryDemolish()
    {
        if (_selectedCell == null) return;
        if (_selectedCell.State != CellSelectionInfo.CellState.Occupied) return;

        foreach (var cost in _selectedCell.Building.Costs)
            _inventory.AddItem(cost.Item, cost.Amount);

        _selectedCell.Surface.DemolishBuilding(_selectedCell.X, _selectedCell.Y);
        _selectedCell.State = CellSelectionInfo.CellState.Free;
        _selectedCell.Building = null;
        _selectedPanel.ShowEmpty();
    }

    private void TryRepair()
    {
        if (_selectedCell == null) return;
        if (_selectedCell.State != CellSelectionInfo.CellState.Obstacle) return;
        if (!CanAfford(_selectedCell.Obstacle.RepairCosts)) return;

        foreach (var cost in _selectedCell.Obstacle.RepairCosts)
            _inventory.RemoveItem(cost.Item, cost.Amount);

        _selectedCell.Surface.RemoveObstacle(_selectedCell.Obstacle);
        _selectedCell.State = CellSelectionInfo.CellState.Free;
        _selectedCell.Obstacle = null;
        _selectedPanel.ShowEmpty();
    }

    // --- Stats ---

    private void RefreshStats()
    {
        float production = GetSurfaceProduction() + (PowerGrid.Instance?.TotalProduction ?? 0f);
        float demand = PowerGrid.Instance?.TotalDemand ?? 0f;
        float balance = production - demand;

        if (_productionLabel != null) _productionLabel.text = $"Produktion: {production:F1} MW";
        if (_demandLabel != null) _demandLabel.text = $"Verbrauch: {demand:F1} MW";

        if (_balanceLabel != null)
        {
            _balanceLabel.text = $"Bilanz: {balance:+F1;-F1} MW";
            _balanceLabel.color = balance >= 0f ? _colorSurplus : _colorDeficit;
        }

        if (_statusIcon != null)
            _statusIcon.color = balance >= 0f ? _colorSurplus : _colorDeficit;
    }

    private float GetSurfaceProduction()
    {
        float total = 0f;
        foreach (var s in _surfaces) total += s.GetTotalProduction();
        return total;
    }

    // --- Helpers ---

    private bool CanAfford(BuildingCost[] costs)
    {
        if (costs == null || _inventory == null) return true;
        foreach (var cost in costs)
            if (_inventory.GetItemCount(cost.Item) < cost.Amount) return false;
        return true;
    }
}