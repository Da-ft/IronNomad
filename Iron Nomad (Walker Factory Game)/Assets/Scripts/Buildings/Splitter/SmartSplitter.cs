using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SmartSplitter : MonoBehaviour, IItemHolder
{
    [Header("Settings")]
    [SerializeField] private float _processTime = 0.5f;
    [SerializeField] private float _gridSize = 2f;

    // Interne Warteschlange FIFO (First in, First out)
    private Queue<ItemDefinition> _inventory = new Queue<ItemDefinition>();

    private GameObject _currentVisual;

    private float _timer;
    // Round Robin Index (merkt sich welchen Ausgang wir zuletzt benutzt haben)
    private int _lastOutputIndex = 0;

    private WalkerGrid _grid;
    private readonly Vector3[] _directions = new Vector3[]
    {
        Vector3.forward,
        Vector3.right,
        Vector3.back,
        Vector3.left,
    };

    private void Start()
    {
        _grid = GetComponentInParent<WalkerGrid>();
    }

    private void Update()
    {
        // Wenn nichts drin ist, tu nichts
        if (_inventory.Count == 0) return;

        _timer += Time.deltaTime;

        if (_timer >= _processTime)
        {
            TryDistributeItem();
        }
    }

    private void TryDistributeItem()
    {
        if (_grid == null) return;

        // Ältestes Item holen ohne es zu löschen
        ItemDefinition itemToPush = _inventory.Peek();

        // Probieren aller 4 Richtungen, beginnend beim nächsten Index
        // --> Gleichmäßige Verteilung (Round Robin)
        int attemps = 0;
        bool success = false;

        while (attemps < 4)
        {
            // Index erhöhen und Modulo 4 rechnen (0, 1, 2, 3, 0, 1...)
            _lastOutputIndex = (_lastOutputIndex + 1) % 4;

            Vector3 checkDir = _directions[_lastOutputIndex];

            // Richtung muss relativ zur Drehung des Splitters sein!
            Vector3 worldDir = transform.TransformDirection(checkDir);
            Vector3 targetPos = transform.position + (worldDir * _gridSize);

            IItemHolder neighbor = _grid.GetHolderAt(targetPos);

            if (neighbor != null)
            {
                if (IsOutputValid(neighbor, worldDir))
                {
                    if (neighbor.TryAcceptItem(itemToPush))
                    {
                        SuccessDispatch();
                        success = true;
                        break;
                    }
                }
            }
            attemps++;
        }
    }

    private bool IsOutputValid(IItemHolder holder, Vector3 directionToNeighbor)
    {
        MonoBehaviour mb = holder as MonoBehaviour;
        if (mb != null)
        {
            float dotProduct = Vector3.Dot(mb.transform.forward, directionToNeighbor);

            if (dotProduct < -0.9f) return false;
        }
        return true;
    }

    private void SuccessDispatch()
    {
        _inventory.Dequeue();

        _timer = 0f;

        if (_currentVisual != null) Destroy(_currentVisual);

        if (_inventory.Count > 0)
        {
            SpawnVisual(_inventory.Peek());
        }
    }

    public bool TryTakeItem(WorldItem item)
    {
        return false;
    }

    public bool TryAcceptItem(ItemDefinition item)
    {
        // Nimmt alles an, solange Puffer nicht voll ist
        if (_inventory.Count >= 5) return false;

        _inventory.Enqueue(item);

        if (_inventory.Count == 1)
        {
            SpawnVisual(item);
        }
        return true;
    }

    private void SpawnVisual(ItemDefinition item)
    {
        if (item.VisualPrefab != null)
        {
            _currentVisual = Instantiate(item.VisualPrefab, transform);
            _currentVisual.transform.localPosition = new Vector3(0, 0.5f, 0); // Etwas erhöht
            _currentVisual.transform.localRotation = Quaternion.identity;

            foreach (var c in _currentVisual.GetComponentsInChildren<Collider>()) Destroy(c);
            Destroy(_currentVisual.GetComponent<WorldItem>());
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, Vector3.one * 1.5f);

        foreach (var dir in _directions)
        {
            Vector3 worldDir = transform.TransformDirection(dir);
            Gizmos.DrawRay(transform.position, worldDir * 2f);
        }
    }
}
