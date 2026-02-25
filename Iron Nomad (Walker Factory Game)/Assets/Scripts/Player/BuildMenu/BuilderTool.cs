using IronNomad.Inputs;
using UnityEngine;

public class BuilderTool : BaseTool
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _cameraRoot;
    [SerializeField] private WalkerGrid _grid;

    [Header("Config")]
    [SerializeField] private float _raycastRange = 10f;

    // State
    private BuildingDefinition _selectedBuilding;
    private GameObject _ghostInstance;
    private int _currentRotation = 0;

    // --- BaseTool ---

    public override void OnEquip()
    {
        _inputReader.RotateEvent += RotateGhost;
        _inputReader.PlaceEvent += TryPlace;
        _inputReader.BuildModeEvent += OpenBuildMenu;

        if (_selectedBuilding != null) SpawnGhost();
    }

    public override void OnUnequip()
    {
        _inputReader.RotateEvent -= RotateGhost;
        _inputReader.PlaceEvent -= TryPlace;
        _inputReader.BuildModeEvent -= OpenBuildMenu;

        DestroyGhost();
    }

    private void Update()
    {
        if (_selectedBuilding == null) return;
        UpdateGhostPosition();
    }

    // --- Build Menu ---

    private void OpenBuildMenu()
    {
        UIManager.Instance.OpenMenu(FindAnyObjectByType<BuildMenu>());
    }

    public void SelectBuilding(BuildingDefinition building)
    {
        _selectedBuilding = building;
        _currentRotation = 0;
        DestroyGhost();
        SpawnGhost();
    }

    // --- Ghost ---

    private void SpawnGhost()
    {
        if (_selectedBuilding?.BuildingPrefab == null) return;

        _ghostInstance = Instantiate(_selectedBuilding.BuildingPrefab);

        if (_selectedBuilding.GhostMaterial != null)
        {
            foreach (var renderer in _ghostInstance.GetComponentsInChildren<Renderer>())
            {
                Material[] mats = new Material[renderer.materials.Length];
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = _selectedBuilding.GhostMaterial;
                renderer.materials = mats;
            }
        }

        foreach (var col in _ghostInstance.GetComponentsInChildren<Collider>())
            col.enabled = false;

        foreach (var mb in _ghostInstance.GetComponentsInChildren<MonoBehaviour>())
            mb.enabled = false;
    }

    private void DestroyGhost()
    {
        if (_ghostInstance != null)
        {
            Destroy(_ghostInstance);
            _ghostInstance = null;
        }
    }

    private void UpdateGhostPosition()
    {
        if (_ghostInstance == null) return;

        Ray ray = new Ray(_cameraRoot.position, _cameraRoot.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, _raycastRange))
        {
            if (!hit.collider.CompareTag("Grid"))
            {
                _ghostInstance.SetActive(false);
                return;
            }

            Vector3 snappedPos = _grid.GetNearestGridPoint(hit.point);
            Vector2Int coords = _grid.WorldToGridCoords(hit.point);

            _ghostInstance.SetActive(true);
            _ghostInstance.transform.position = snappedPos;
            _ghostInstance.transform.rotation = _grid.transform.rotation *
                Quaternion.Euler(0, _currentRotation, 0);

            bool canPlace = _grid.IsCellAvailable(coords);
            foreach (var renderer in _ghostInstance.GetComponentsInChildren<Renderer>())
            {
                renderer.material.color = canPlace
                    ? new Color(0f, 1f, 0f, 0.5f)
                    : new Color(1f, 0f, 0f, 0.5f);
            }
        }
        else
        {
            _ghostInstance.SetActive(false);
        }
    }

    // --- Rotation ---

    private void RotateGhost()
    {
        if (_ghostInstance == null) return;

        _currentRotation = (_currentRotation + 90) % 360;
        _ghostInstance.transform.rotation = _grid.transform.rotation *
            Quaternion.Euler(0, _currentRotation, 0);
    }

    // --- Platzieren ---

    private void TryPlace()
    {
        if (_selectedBuilding == null) return;

        Ray ray = new Ray(_cameraRoot.position, _cameraRoot.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, _raycastRange))
        {
            if (!hit.collider.CompareTag("Grid")) return;

            Vector2Int coords = _grid.WorldToGridCoords(hit.point);

            if (!_grid.IsCellAvailable(coords))
            {
                Debug.Log("Zelle belegt!");
                return;
            }

            Vector3 snappedPos = _grid.GetNearestGridPoint(hit.point);
            Quaternion rotation = _grid.transform.rotation * Quaternion.Euler(0, _currentRotation, 0);

            GameObject building = Instantiate(
                _selectedBuilding.BuildingPrefab,
                snappedPos,
                rotation
            );

            building.transform.SetParent(_grid.transform);

            ConstructibleBuilding constructible = building.GetComponent<ConstructibleBuilding>();
            if (constructible == null)
                constructible = building.AddComponent<ConstructibleBuilding>();
            constructible.Initialize(_selectedBuilding);

            _grid.OccupyCell(snappedPos);
        }
    }
}