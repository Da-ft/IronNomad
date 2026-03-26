using UnityEngine;

public class BuilderTool : BaseTool
{
    [Header("Dependencies")]
    [SerializeField] private WalkerGrid _grid;

    [Header("Config")]
    [SerializeField] private float _raycastRange = 10f;
    [SerializeField] private Material _ghostValidMaterial;
    [SerializeField] private Material _ghostInvalidMaterial;

    private BuildingDefinition _selectedBuilding;
    private GameObject _ghostInstance;
    private int _currentRotation = 0;

    private BeltPlacementHandler _beltHandler;
    private bool UsesBeltPlacement => _selectedBuilding != null && _selectedBuilding.UsesBeltPlacement;

    public override void OnEquip()
    {
        InputEvents.OnPlace += OnPlace;
        InputEvents.OnRotate += RotateGhost;
    }

    public override void OnUnequip()
    {
        InputEvents.OnPlace -= OnPlace;
        InputEvents.OnRotate -= RotateGhost;

        _beltHandler?.Cancel();
        _beltHandler = null;
        DestroyGhost();
        _selectedBuilding = null;
    }

    private void Update()
    {
        if (_selectedBuilding == null) return;
        if (UsesBeltPlacement) _beltHandler?.Tick();
        else UpdateGhostPosition();
    }

    public void SelectBuilding(BuildingDefinition definition)
    {
        _beltHandler?.Cancel();
        _beltHandler = null;
        DestroyGhost();

        _selectedBuilding = definition;
        _currentRotation = 0;

        if (UsesBeltPlacement)
        {
            _beltHandler = new BeltPlacementHandler(
                _grid, _selectedBuilding, _cameraRoot, _raycastRange, _ghostValidMaterial);
        }
        else
        {
            SpawnGhost();
        }
    }

    private void OnPlace()
    {
        if (_selectedBuilding == null) return;
        if (UsesBeltPlacement) _beltHandler.OnPlace();
        else TryPlaceBuilding();
    }

    private void RotateGhost()
    {
        if (_ghostInstance == null) return;
        _currentRotation = (_currentRotation + 90) % 360;
        _ghostInstance.transform.rotation = _grid.transform.rotation * Quaternion.Euler(0, _currentRotation, 0);
    }

    private void TryPlaceBuilding()
    {
        if (!Physics.Raycast(GetCameraRay(), out RaycastHit hit, _raycastRange)) return;
        if (!hit.collider.CompareTag("Grid")) return;

        Vector2Int coords = _grid.WorldToGridCoords(hit.point);
        Vector2Int gridSize = _selectedBuilding.GridSize;

        if (!_grid.IsCellAvailable(coords, gridSize, _currentRotation))
        {
            Debug.Log("[BuilderTool] Zelle belegt!");
            return;
        }

        Vector3 snappedPos = _grid.GetNearestGridPoint(hit.point);
        Quaternion rotation = _grid.transform.rotation * Quaternion.Euler(0, _currentRotation, 0);

        GameObject building = Instantiate(_selectedBuilding.BuildingPrefab, snappedPos, rotation);
        building.transform.SetParent(_grid.transform);

        ConstructibleBuilding constructible = building.GetComponent<ConstructibleBuilding>()
            ?? building.AddComponent<ConstructibleBuilding>();
        constructible.Initialize(_selectedBuilding, _currentRotation);

        _grid.OccupyCell(snappedPos, gridSize, _currentRotation);
    }

    private void SpawnGhost()
    {
        if (_selectedBuilding?.BuildingPrefab == null) return;
        _ghostInstance = Instantiate(_selectedBuilding.BuildingPrefab);

        foreach (var r in _ghostInstance.GetComponentsInChildren<Renderer>())
        {
            var mats = new Material[r.materials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = _selectedBuilding.GhostMaterial != null ? _selectedBuilding.GhostMaterial : r.materials[i];
            r.materials = mats;
        }

        foreach (var col in _ghostInstance.GetComponentsInChildren<Collider>()) col.enabled = false;
        foreach (var mb in _ghostInstance.GetComponentsInChildren<MonoBehaviour>()) mb.enabled = false;
    }

    private void DestroyGhost()
    {
        if (_ghostInstance != null) { Destroy(_ghostInstance); _ghostInstance = null; }
    }

    private void UpdateGhostPosition()
    {
        if (_ghostInstance == null) return;

        if (!Physics.Raycast(GetCameraRay(), out RaycastHit hit, _raycastRange)
            || !hit.collider.CompareTag("Grid"))
        {
            _ghostInstance.SetActive(false);
            return;
        }

        Vector3 snappedPos = _grid.GetNearestGridPoint(hit.point);
        Vector2Int coords = _grid.WorldToGridCoords(hit.point);
        bool canPlace = _grid.IsCellAvailable(coords, _selectedBuilding.GridSize, _currentRotation);

        _ghostInstance.SetActive(true);
        _ghostInstance.transform.position = snappedPos;
        _ghostInstance.transform.rotation = _grid.transform.rotation * Quaternion.Euler(0, _currentRotation, 0);

        Material mat = canPlace ? _ghostValidMaterial : _ghostInvalidMaterial;
        if (mat != null)
        {
            foreach (var r in _ghostInstance.GetComponentsInChildren<Renderer>())
            {
                var mats = new Material[r.materials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                r.materials = mats;
            }
        }
    }
}