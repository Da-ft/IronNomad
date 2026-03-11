using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Zeigt eine Walker-Oberfläche als interaktives Grid.
/// Feuert OnCellSelected wenn eine Zelle angeklickt wird.
/// Placement-Logic sitzt in WalkerPowerMenuUI.
/// </summary>
public class WalkerSurfaceGridUI : MonoBehaviour
{
    [Header("Surface Definition")]
    [SerializeField] private WalkerSurface _surface;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _titleLabel;
    [SerializeField] private GridLayoutGroup _gridLayout;
    [SerializeField] private GameObject _cellPrefab;

    [Header("Colors")]
    [SerializeField] private Color _colorFree = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private Color _colorOccupied = new Color(0.2f, 0.8f, 0.3f, 0.8f);
    [SerializeField] private Color _colorObstacle = new Color(0.8f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color _colorSelected = new Color(1f, 1f, 0f, 0.8f);
    [SerializeField] private Color _colorHover = new Color(1f, 1f, 0f, 0.4f);

    public event System.Action<CellSelectionInfo> OnCellSelected;
    public event System.Action OnPlacementChanged;

    public WalkerSurface Surface => _surface;

    // --- State ---

    private enum CellState { Free, Occupied, Obstacle }

    private CellState[] _cellStates;
    private BuildingDefinition[] _cellBuildings;
    private WalkerObstacle[] _cellObstacles;
    private Image[] _cellImages;
    private int _selectedIdx = -1;

    // --- Unity ---

    private void Start()
    {
        if (_titleLabel != null) _titleLabel.text = _surface.SurfaceName;
        BuildGrid();
    }

    // --- Public API (aufgerufen von WalkerPowerMenuUI) ---

    public void PlaceBuilding(int x, int y, BuildingDefinition def)
    {
        for (int cy = y; cy < y + def.GridSize.y; cy++)
            for (int cx = x; cx < x + def.GridSize.x; cx++)
            {
                int idx = CellIndex(cx, cy);
                _cellStates[idx] = CellState.Occupied;
                _cellBuildings[idx] = def;
                RefreshCell(idx);
            }
        OnPlacementChanged?.Invoke();
    }

    public void DemolishBuilding(int x, int y)
    {
        BuildingDefinition def = _cellBuildings[CellIndex(x, y)];
        if (def == null) return;

        // Ursprung finden
        int originX = x, originY = y;
        for (int cy = 0; cy < _surface.GridSize.y; cy++)
            for (int cx = 0; cx < _surface.GridSize.x; cx++)
                if (_cellBuildings[CellIndex(cx, cy)] == def) { originX = cx; originY = cy; goto found; }
            found:

        for (int cy = originY; cy < originY + def.GridSize.y; cy++)
            for (int cx = originX; cx < originX + def.GridSize.x; cx++)
            {
                int idx = CellIndex(cx, cy);
                _cellStates[idx] = CellState.Free;
                _cellBuildings[idx] = null;
                RefreshCell(idx);
            }

        _selectedIdx = -1;
        OnPlacementChanged?.Invoke();
    }

    public void RemoveObstacle(WalkerObstacle obs)
    {
        for (int cy = obs.GridPosition.y; cy < obs.GridPosition.y + obs.GridSize.y; cy++)
            for (int cx = obs.GridPosition.x; cx < obs.GridPosition.x + obs.GridSize.x; cx++)
            {
                if (!InBounds(cx, cy)) continue;
                int idx = CellIndex(cx, cy);
                _cellStates[idx] = CellState.Free;
                _cellObstacles[idx] = null;
                RefreshCell(idx);
            }
        OnPlacementChanged?.Invoke();
    }

    public bool CanPlace(int x, int y, BuildingDefinition def)
    {
        for (int cy = y; cy < y + def.GridSize.y; cy++)
            for (int cx = x; cx < x + def.GridSize.x; cx++)
            {
                if (!InBounds(cx, cy)) return false;
                if (_cellStates[CellIndex(cx, cy)] != CellState.Free) return false;
            }
        return true;
    }

    public float GetTotalProduction()
    {
        if (_cellBuildings == null) return 0f;
        float total = 0f;
        var counted = new HashSet<BuildingDefinition>();
        foreach (var def in _cellBuildings)
        {
            if (def == null || counted.Contains(def)) continue;
            counted.Add(def);
            total += def.PowerProduction * _surface.BonusMultiplier;
        }
        return total;
    }

    // --- Grid Setup ---

    private void BuildGrid()
    {
        int cols = _surface.GridSize.x;
        int rows = _surface.GridSize.y;
        int count = cols * rows;

        _cellStates = new CellState[count];
        _cellBuildings = new BuildingDefinition[count];
        _cellObstacles = new WalkerObstacle[count];
        _cellImages = new Image[count];

        _gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _gridLayout.constraintCount = cols;

        foreach (Transform child in _gridLayout.transform) Destroy(child.gameObject);

        if (_surface.Obstacles != null)
            foreach (var obs in _surface.Obstacles)
                SetObstacleOnGrid(obs);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int idx = CellIndex(x, y);
                GameObject obj = Instantiate(_cellPrefab, _gridLayout.transform);
                Image img = obj.GetComponent<Image>() ?? obj.GetComponentInChildren<Image>();
                _cellImages[idx] = img;
                RefreshCell(idx);

                int cx = x, cy = y;
                var trigger = obj.GetComponent<EventTrigger>() ?? obj.AddComponent<EventTrigger>();
                AddTrigger(trigger, EventTriggerType.PointerEnter, _ => OnCellHover(cx, cy));
                AddTrigger(trigger, EventTriggerType.PointerExit, _ => OnCellExit(cx, cy));
                AddTrigger(trigger, EventTriggerType.PointerClick, _ => OnCellClick(cx, cy));
            }
        }
    }

    private void SetObstacleOnGrid(WalkerObstacle obs)
    {
        for (int y = obs.GridPosition.y; y < obs.GridPosition.y + obs.GridSize.y; y++)
            for (int x = obs.GridPosition.x; x < obs.GridPosition.x + obs.GridSize.x; x++)
            {
                if (!InBounds(x, y)) continue;
                int idx = CellIndex(x, y);
                _cellStates[idx] = CellState.Obstacle;
                _cellObstacles[idx] = obs;
            }
    }

    // --- Input ---

    private void OnCellHover(int x, int y)
    {
        int idx = CellIndex(x, y);
        if (idx != _selectedIdx && _cellStates[idx] == CellState.Free)
            _cellImages[idx].color = _colorHover;
    }

    private void OnCellExit(int x, int y)
    {
        int idx = CellIndex(x, y);
        if (idx != _selectedIdx) RefreshCell(idx);
    }

    private void OnCellClick(int x, int y)
    {
        // Alten Selected zurücksetzen
        if (_selectedIdx >= 0) RefreshCell(_selectedIdx);

        int idx = CellIndex(x, y);
        _selectedIdx = idx;
        _cellImages[idx].color = _colorSelected;

        var info = new CellSelectionInfo
        {
            X = x,
            Y = y,
            State = _cellStates[idx] switch
            {
                CellState.Occupied => CellSelectionInfo.CellState.Occupied,
                CellState.Obstacle => CellSelectionInfo.CellState.Obstacle,
                _ => CellSelectionInfo.CellState.Free
            },
            Building = _cellBuildings[idx],
            Obstacle = _cellObstacles[idx],
            Surface = this
        };

        OnCellSelected?.Invoke(info);
    }

    // --- Helpers ---

    private void RefreshCell(int idx)
    {
        if (_cellImages[idx] == null) return;
        _cellImages[idx].color = _cellStates[idx] switch
        {
            CellState.Occupied => _colorOccupied,
            CellState.Obstacle => _colorObstacle,
            _ => _colorFree
        };
    }

    private int CellIndex(int x, int y) => y * _surface.GridSize.x + x;
    private bool InBounds(int x, int y) => x >= 0 && x < _surface.GridSize.x && y >= 0 && y < _surface.GridSize.y;

    private void AddTrigger(EventTrigger t, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(action);
        t.triggers.Add(entry);
    }
}