using UnityEngine;
using IronNomad.Inputs;

public class BuilderTool : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _cameraRoot;
    [SerializeField] private WalkerGrid _grid;

    [Header("Config")]
    [SerializeField] private float _raycastRange = 10f;

    // State
    private bool _isInBuildMode = false;
    private BuildingDefinition _selectedBuilding;
    private GameObject _ghostInstance;
    private int _currentRotation = 0; // 0, 90, 180, 270

    [Header("Debug")]
    [SerializeField] private BuildingDefinition _debugBuilding;

    private void OnEnable()
    {
        _inputReader.BuildModeEvent += ToggleBuildMode;
        _inputReader.RotateEvent += RotateGhost;
        _inputReader.PlaceEvent += TryPlace;
    }

    private void OnDisable()
    {
        _inputReader.BuildModeEvent -= ToggleBuildMode;
        _inputReader.RotateEvent -= RotateGhost;
        _inputReader.PlaceEvent -= TryPlace;

        DestroyGhost();
    }

    private void Update()
    {
        if (_debugBuilding != null && _selectedBuilding == null)
            SelectBuilding(_debugBuilding);

        if (!_isInBuildMode || _selectedBuilding == null) return;

        UpdateGhostPosition();
    }

    // --- Build Mode ---

    private void ToggleBuildMode()
    {
        _isInBuildMode = !_isInBuildMode;
        Debug.Log($"Baumodus: {_isInBuildMode} | Gebäude: {_selectedBuilding?.DisplayName ?? "KEINS"}");

        if (_isInBuildMode)
        {
            if (_selectedBuilding != null) SpawnGhost();
        }
        else
        {
            DestroyGhost();
        }
    }

    // Wird später vom Bau-Menü aufgerufen
    public void SelectBuilding(BuildingDefinition building)
    {
        _selectedBuilding = building;
        _currentRotation = 0;

        DestroyGhost();

        // Nur Ghost spawnen wenn wir bereits im Baumodus sind
        if (_isInBuildMode) SpawnGhost();
    }

    // --- Ghost ---

    private void SpawnGhost()
    {
        if (_selectedBuilding?.BuildingPrefab == null) return;

        _ghostInstance = Instantiate(_selectedBuilding.BuildingPrefab);

        // Ghost Material auf alle Renderer anwenden
        if (_selectedBuilding.GhostMaterial != null)
        {
            foreach (var renderer in _ghostInstance.GetComponentsInChildren<Renderer>())
            {
                // Alle Material-Slots mit Ghost Material ersetzen
                Material[] mats = new Material[renderer.materials.Length];
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = _selectedBuilding.GhostMaterial;
                renderer.materials = mats;
            }
        }

        // Collider deaktivieren damit der Ghost nicht mit der Welt interagiert
        foreach (var col in _ghostInstance.GetComponentsInChildren<Collider>())
            col.enabled = false;

        // Alle MonoBehaviours deaktivieren damit keine Logik läuft
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

            // Farbe je nach Verfügbarkeit (optional: zwei Ghost Materials)
            bool canPlace = _grid.IsCellAvailable(coords);
            foreach (var renderer in _ghostInstance.GetComponentsInChildren<Renderer>())
            {
                // Tint: Grün = frei, Rot = belegt
                renderer.material.color = canPlace
                    ? new Color(0f, 1f, 0f, 0.5f)
                    : new Color(1f, 0f, 0f, 0.5f);
            }
        }
        else
        {
            // Kein Grid getroffen - Ghost verstecken
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

            // Einheitlich: BuilderTool belegt immer die Zelle
            _grid.OccupyCell(snappedPos);

            Debug.Log($"{_selectedBuilding.DisplayName} platziert auf {coords}");
        }
    }
}