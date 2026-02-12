using UnityEngine;

public class OutputShaft : MonoBehaviour, IItemHolder
{
    [Header("State")]
    public ItemDefinition CurrentItem { get; private set; }

    [Header("Visuals")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Transform _ejectPoint;
    [SerializeField] private LayerMask _connectionLayer;
    [SerializeField] private float _gridSize = 2f;

    private WalkerGrid _grid;

    private GameObject _currentVisualObj;

    private void Start()
    {
        _grid = GetComponentInParent<WalkerGrid>();
    }

    private void Update()
    {
        if (CurrentItem != null)
        {
            TryPushToNext();
        }
    }

    private void TryPushToNext()
    {
        if (_grid == null) return;

        // Wir berechnen die Koordinate genau EINEN Schritt vor uns
        Vector3 targetPos = transform.position + (transform.forward * _gridSize);

        // Debugging: Zeichne eine rote Linie im Editor, wohin der Schacht guckt!
        Debug.DrawLine(transform.position, targetPos, Color.red);

        IItemHolder nextHolder = _grid.GetHolderAt(targetPos);

        if (nextHolder != null)
        {
            if (nextHolder.TryAcceptItem(CurrentItem))
            {
                Debug.Log("OutputShaft: Übergabe erfolgreich!");
                ClearContent();
            }
            else
            {
                // Das Band ist voll oder blockiert
                Debug.Log("OutputShaft: Nachbar ist voll.");
            }
        }
        else
        {
            // Das ist dein aktuelles Problem: Er findet niemanden.
            // Wir lassen uns mal ausgeben, wo er sucht:
            Vector2Int gridCoords = _grid.WorldToGridCoords(targetPos);
            Debug.LogWarning($"OutputShaft: Suche auf Grid {gridCoords} - NIEMAND DA!");
        }
    }

    public bool TryAcceptItem(ItemDefinition item)
    {
        if (CurrentItem != null) return false;

        CurrentItem = item;
        SpawnVisuals();
        return true;
    }

    public bool TryDeposit(ItemDefinition item)
    {
        if (CurrentItem != null) return false;

        CurrentItem = item;
        SpawnVisuals();
        return true;
    }

    public bool TryTakeItem(WorldItem itemScript)
    {
        if (_currentVisualObj != itemScript.gameObject) return false;

        CurrentItem = null;
        _currentVisualObj = null;

        return true;
    }

    private void SpawnVisuals()
    {
        if (CurrentItem != null && CurrentItem.VisualPrefab != null)
        {
            _currentVisualObj = Instantiate(CurrentItem.VisualPrefab, _spawnPoint);
            _currentVisualObj.transform.localPosition = Vector3.zero;
            _currentVisualObj.transform.localRotation = Quaternion.identity;

            // WICHTIG: Wir kleben das WorldItem Script drauf und initialisieren es
            WorldItem worldItem = _currentVisualObj.AddComponent<WorldItem>();

            // Falls das Prefab noch keinen Collider hat, fügen wir einen hinzu
            if (!_currentVisualObj.TryGetComponent<Collider>(out var col))
            {
                var box = _currentVisualObj.AddComponent<BoxCollider>();
                box.size = Vector3.one * 0.5f;
            }

            worldItem.Initialize(CurrentItem, this);
        }
    }

    private void ClearContent()
    {
        // Löscht Visuelles und Daten, nachdem es ans Band übergeben wurde
        if (_currentVisualObj != null) Destroy(_currentVisualObj);
        CurrentItem = null;
    }

    private void OnDrawGizmos()
    {
        // Zeichnet einen gelben Pfeil im Editor, damit du weißt, wo "Vorne" ist
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * _gridSize);
        Gizmos.DrawWireSphere(transform.position + transform.forward * _gridSize, 0.2f);
    }
}