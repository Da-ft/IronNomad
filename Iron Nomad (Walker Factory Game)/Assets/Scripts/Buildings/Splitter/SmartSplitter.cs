using System.Collections.Generic;
using UnityEngine;

public class SmartSplitter : BaseGridMachine
{
    [Header("Settings")]
    [SerializeField] private float _processTime = 0.5f;

    private Queue<ItemDefinition> _inventory = new Queue<ItemDefinition>();
    private GameObject _currentVisual;
    private float _timer;
    private int _lastOutputIndex = 0;

    private readonly Vector3[] _directions = new Vector3[]
    {
        Vector3.forward, Vector3.right, Vector3.back, Vector3.left,
    };

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
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

        ItemDefinition itemToPush = _inventory.Peek();
        int attempts = 0;

        while (attempts < 4)
        {
            _lastOutputIndex = (_lastOutputIndex + 1) % 4;
            Vector3 checkDir = _directions[_lastOutputIndex];

            // Lokale Richtung in Weltrichtung umwandeln
            Vector3 worldDir = transform.TransformDirection(checkDir);

            // Nutze GridStepSize
            Vector3 targetPos = transform.position + (worldDir * GridStepSize);

            IItemHolder neighbor = _grid.GetHolderAt(targetPos);

            if (neighbor != null && IsOutputValid(neighbor, worldDir))
            {
                if (neighbor.TryAcceptItem(itemToPush, _currentVisual))
                {
                    SuccessDispatch(true);
                    break;
                }
            }
            attempts++;
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

    private void SuccessDispatch(bool visualPassedOn)
    {
        _inventory.Dequeue();
        _timer = 0f;
        if (visualPassedOn) _currentVisual = null;
        else
        {
            if(_currentVisual != null) Destroy(_currentVisual);
        }

        if (_inventory.Count > 0) SpawnVisual(_inventory.Peek());
    }

    public override bool TryTakeItem(WorldItem item)
    {
        return false; // Splitter aktuell nicht manuell entnehmbar
    }

    public override bool TryAcceptItem(ItemDefinition item, GameObject visualObj)
    {
        if (_inventory.Count >= 5) return false;

        if(visualObj != null) Destroy(visualObj);

        _inventory.Enqueue(item);
        if (_inventory.Count == 1) SpawnVisual(item);
        return true;
    }

    private void SpawnVisual(ItemDefinition item)
    {
        if (item.VisualPrefab != null)
        {
            _currentVisual = Instantiate(item.VisualPrefab, transform);
            _currentVisual.transform.localPosition = new Vector3(0, 0.5f, 0);
            _currentVisual.transform.localRotation = Quaternion.identity;

            _currentVisual.transform.localScale = Vector3.one;

            foreach (var c in _currentVisual.GetComponentsInChildren<Collider>()) Destroy(c);
            Destroy(_currentVisual.GetComponent<WorldItem>());
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, Vector3.one * 1.5f);

        float size = Application.isPlaying ? GridStepSize : 2f;
        foreach (var dir in _directions)
        {
            Vector3 worldDir = transform.TransformDirection(dir);
            Gizmos.DrawRay(transform.position, worldDir * size);
        }
    }
}