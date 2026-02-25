using IronNomad.Inputs;
using System.Collections.Generic;
using UnityEngine;

public class BuilderTool : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _cameraRoot;
    [SerializeField] private WalkerGrid _grid;
    //[SerializeField] private InventorySystem _inventory;

    [Header("Config")]
    [SerializeField] private float _raycastRange = 10f;

    [Header("Demolish")]
    [SerializeField] private Material _demolishHighlightMaterial;
    [SerializeField] private LayerMask _demolishLayer;

    // Build State
    private bool _isInBuildMode = false;
    private BuildingDefinition _selectedBuilding;
    private GameObject _ghostInstance;
    private int _currentRotation = 0;

    // Demolish State
    private bool _isInDemolishMode = false;
    private GameObject _demolishTarget;
    private List<Material[]> _originalMaterials = new List<Material[]>();

    private void OnEnable()
    {
        _inputReader.BuildModeEvent += ToggleBuildMode;
        _inputReader.RotateEvent += RotateGhost;
        _inputReader.PlaceEvent += TryPlace;
        _inputReader.DemolishEvent += ToggleDemolishMode;
        _inputReader.DemolishConfirmEvent += TryDemolishConfirm;
    }

    private void OnDisable()
    {
        _inputReader.BuildModeEvent -= ToggleBuildMode;
        _inputReader.RotateEvent -= RotateGhost;
        _inputReader.PlaceEvent -= TryPlace;
        _inputReader.DemolishEvent -= ToggleDemolishMode;
        _inputReader.DemolishConfirmEvent -= TryDemolishConfirm;

        DestroyGhost();
    }

    private void Update()
    {
        if (_isInDemolishMode)
        {
            UpdateDemolishPreview();
            return;
        }

        if (!_isInBuildMode || _selectedBuilding == null) return;
        UpdateGhostPosition();
    }

    // --- Build Mode ---

    private void ToggleBuildMode()
    {
        if (_isInDemolishMode)
        {
            // Demolish ausschalten, Build einschalten
            _isInDemolishMode = false;
            ClearDemolishHighlight();
            _isInBuildMode = true;
            if (_selectedBuilding != null) SpawnGhost();
            return;
        }

        _isInBuildMode = !_isInBuildMode;
        if (!_isInBuildMode) DestroyGhost();
        else if (_selectedBuilding != null) SpawnGhost();
    }

    public void EnableBuildMode()
    {
        _isInBuildMode = true;
        if (_selectedBuilding != null) SpawnGhost();
    }

    public void SelectBuilding(BuildingDefinition building)
    {
        _selectedBuilding = building;
        _currentRotation = 0;
        DestroyGhost();
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
        if (!_isInBuildMode || _ghostInstance == null) return;

        _currentRotation = (_currentRotation + 90) % 360;
        _ghostInstance.transform.rotation = _grid.transform.rotation *
            Quaternion.Euler(0, _currentRotation, 0);
    }

    // --- Platzieren ---

    private void TryPlace()
    {
        if (!_isInBuildMode || _selectedBuilding == null) return;

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

    // --- Demolish Mode ---

    private void ToggleDemolishMode()
    {
        if (_isInBuildMode)
        {
            // Build ausschalten, Demolish einschalten
            _isInBuildMode = false;
            DestroyGhost();
            _isInDemolishMode = true;
            return;
        }

        _isInDemolishMode = !_isInDemolishMode;
        if (!_isInDemolishMode) ClearDemolishHighlight();
    }

    private void UpdateDemolishPreview()
    {
        Ray ray = new Ray(_cameraRoot.position, _cameraRoot.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, _raycastRange, _demolishLayer))
        {
            ConstructibleBuilding target = hit.collider.GetComponentInParent<ConstructibleBuilding>();

            if (target != null && target.gameObject != _demolishTarget)
            {
                ClearDemolishHighlight();
                HighlightDemolishTarget(target.gameObject);
            }
        }
        else
        {
            ClearDemolishHighlight();
        }
    }

    private void HighlightDemolishTarget(GameObject target)
    {
        _demolishTarget = target;
        _originalMaterials.Clear();

        foreach (var renderer in target.GetComponentsInChildren<Renderer>())
        {
            _originalMaterials.Add(renderer.materials);

            Material[] redMats = new Material[renderer.materials.Length];
            for (int i = 0; i < redMats.Length; i++)
                redMats[i] = _demolishHighlightMaterial;
            renderer.materials = redMats;
        }
    }

    private void ClearDemolishHighlight()
    {
        if (_demolishTarget == null) return;

        Renderer[] renderers = _demolishTarget.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length && i < _originalMaterials.Count; i++)
            renderers[i].materials = _originalMaterials[i];

        _demolishTarget = null;
        _originalMaterials.Clear();
    }

    private void TryDemolish(ConstructibleBuilding building)
    {
        //// Ressourcen zurückgeben
        //if (building.Definition != null && _inventory != null)
        //{
        //    foreach (var cost in building.Definition.Costs)
        //    {
        //        _inventory.AddItem(cost.Item, cost.Amount);
        //        Debug.Log($"Refund: {cost.Amount}x {cost.Item.Name}");
        //    }
        //}

        // Zelle freigeben
        _grid.FreeCell(building.transform.position);

        // Gebäude zerstören
        building.Demolish();

        // Demolish Mode beenden
        _isInDemolishMode = false;
        ClearDemolishHighlight();
    }

    private void TryDemolishConfirm()
    {
        if (!_isInDemolishMode || _demolishTarget == null) return;

        ConstructibleBuilding building = _demolishTarget.GetComponent<ConstructibleBuilding>();
        if (building != null) TryDemolish(building);
    }
}