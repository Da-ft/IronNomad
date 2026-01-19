using UnityEngine;
using IronNomad.Inputs;

public class BuilderTool : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _camTransform; 

    [Header("Settings")]
    [SerializeField] private GameObject _buildingPrefab; 
    [SerializeField] private GameObject _ghostPrefab;    
    [SerializeField] private float _reachRange = 10f;    
    [SerializeField] private LayerMask _groundLayer;     

    private GameObject _currentGhost;
    private WalkerGrid _targetGrid; 
    private Vector3 _targetPosition;  

    private void OnEnable()
    {
        if (_inputReader == null)
        {
            Debug.LogError("BuilderTool: InputReader fehlt!");
            enabled = false;
            return;
        }

        _inputReader.BuildEvent += OnBuild;

        // Ghost einmalig erstellen und verstecken
        if (_ghostPrefab != null)
        {
            _currentGhost = Instantiate(_ghostPrefab);
            _currentGhost.SetActive(false);

            // WICHTIG: Collider vom Ghost entfernen, sonst trifft der Raycast den Ghost statt den Boden!
            foreach (var c in _currentGhost.GetComponentsInChildren<Collider>()) Destroy(c);
        }
    }

    private void OnDisable()
    {
        if (_inputReader != null) _inputReader.BuildEvent -= OnBuild;
        if (_currentGhost != null) Destroy(_currentGhost);
    }

    private void Update()
    {
        HandleRaycast();
    }

    private void HandleRaycast()
    {
        // Ein Strahl genau aus der Mitte der Kamera
        Ray ray = new Ray(_camTransform.position, _camTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, _reachRange, _groundLayer))
        {
            // Haben wir ein Grid getroffen?
            if (hit.collider.TryGetComponent(out WalkerGrid grid))
            {
                _targetGrid = grid;

                // Frag das Grid: Wo ist der nächste Rasterpunkt für diesen Treffer?
                _targetPosition = grid.GetNearestGridPoint(hit.point);

                // Ghost anzeigen und bewegen
                if (_currentGhost != null)
                {
                    _currentGhost.SetActive(true);
                    _currentGhost.transform.position = _targetPosition;

                    // Der Ghost dreht sich mit dem Walker mit WIUWIUWIU
                    _currentGhost.transform.rotation = grid.transform.rotation;
                }
                return; // Hier brechen wir ab, weil wir erfolgreich waren, so wie ich nachdem ich mit dem Spiel fertig bin roflmaolol
            }
        }

        // Wenn wir hier landen, haben wir nichts (oder das Falsche) getroffen
        _targetGrid = null;
        if (_currentGhost != null) _currentGhost.SetActive(false);
    }

    private void OnBuild()
    {
        // Nur bauen, wenn wir ein gültiges Grid im Visier haben
        if (_targetGrid != null && _buildingPrefab != null)
        {
            // 1. Objekt erzeugen
            GameObject newBuilding = Instantiate(_buildingPrefab, _targetPosition, _targetGrid.transform.rotation);

            // 2. WICHTIG: Parenting!
            // Damit das Gebäude mitfährt, muss es ein Kind des Walkers werden.
            newBuilding.transform.SetParent(_targetGrid.transform);
        }
    }
}