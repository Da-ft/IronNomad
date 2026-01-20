using IronNomad.Inputs;
using UnityEngine;
using UnityEngine.UIElements;

public class BuilderTool : MonoBehaviour
{
    private enum BuildMode { None, Building, Demolishing }

    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _camTransform;

    [Header("Config")]
    [SerializeField] private GameObject _ghostPrefab;
    [SerializeField] private GameObject _buildingPrefab;
    [SerializeField] private LayerMask _buildLayer;
    [SerializeField] private LayerMask _demolishLayer;

    private BuildMode _currentMode = BuildMode.None;
    private GameObject _currentGhost;
    private WalkerGrid _targetGrid;
    private Vector3 _targetPosition;

    private void OnEnable()
    {
        _inputReader.ToggleBuildEvent += ToggleMode;
        _inputReader.BuildEvent += ExecuteAction;
    }

    private void OnDisable()
    {
        if (_currentGhost != null) Destroy(_currentGhost);
    }

    private void ToggleMode()
    {
        if (_currentMode == BuildMode.None)
        {
            _currentMode = BuildMode.Building;
            if (_ghostPrefab) _currentGhost = Instantiate(_ghostPrefab);
        }
        else
        {
            _currentMode = BuildMode.None;
            if (_currentGhost) Destroy(_currentGhost);
        }
    }

    private void Update()
    {
        if (_currentMode == BuildMode.None) return;

        if (_currentMode == BuildMode.Building) HandleBuildPreview();
        if (_currentMode == BuildMode.Demolishing) HandleDemolishPreview();
    }

    private void HandleBuildPreview()
    {
        Ray ray = new Ray(_camTransform.position, _camTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 10f, _buildLayer))
        {
            if (hit.collider.TryGetComponent(out WalkerGrid grid))
            {
                _targetGrid = grid;
                _targetPosition = grid.GetNearestGridPoint(hit.point);

                if (_currentGhost)
                {
                    _currentGhost.SetActive(true);
                    _currentGhost.transform.position = _targetPosition;
                    _currentGhost.transform.rotation = grid.transform.rotation;

                    // TODO: Prüfen ob Platz belegt ist
                }
                return;
            }
        }
        if (_currentGhost) _currentGhost.SetActive(false);
    }

    private void HandleDemolishPreview()
    {
        // TODO: Gebäude fürs Abreißen highlighten
    }

    private void ExecuteAction()
    {
        if (_currentMode == BuildMode.Building)
        {
            if (_targetGrid != null)
            {
                // TODO: Inventory cost check
                GameObject building = Instantiate(_buildingPrefab, _targetPosition, _targetGrid.transform.rotation);
                building.transform.SetParent(_targetGrid.transform);

                // Add component, to dismantle later
                building.AddComponent<ConstructibleBuilding>();

                building.layer = LayerMask.NameToLayer("Interactable");
            }
        }

        else if (_currentMode == BuildMode.Demolishing)
        {
            // Raycast auf Gebäude
            Ray ray = new Ray(_camTransform.position, _camTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 10f, _demolishLayer))
            {
                if (hit.collider.TryGetComponent(out IConstructible building))
                {
                    building.Demolish(); // TODO: Refund resources + Destroy
                }
            }
        }
    }
}