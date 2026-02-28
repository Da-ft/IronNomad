using System.Collections.Generic;
using UnityEngine;

public class SmartSplitter : BaseGridMachine
{
    [Header("Settings")]
    [SerializeField] private float _moveSpeed = 3f;

    private enum ItemPhase { None, ToCenter, ToExit }

    private ItemDefinition _currentItem;
    private GameObject _currentVisual;
    private ItemPhase _phase = ItemPhase.None;

    private Vector3 _targetPos;
    private Vector3 _centerLocal;

    private IItemHolder _exitNeighbor;
    private Vector3 _exitDir;

    private int _lastOutputIndex = 0;

    private float _retryTimer = 0f;
    private const float RetryInterval = 0.2f;

    private readonly Vector3[] _directions = new Vector3[]
    {
        Vector3.forward, Vector3.right, Vector3.back, Vector3.left,
    };

    protected override void Start()
    {
        base.Start();
        _centerLocal = Vector3.up * 0.5f;
    }

    private void Update()
    {
        if (_currentItem == null) return;

        if (_phase == ItemPhase.None)
        {
            _retryTimer += Time.deltaTime;
            if (_retryTimer >= RetryInterval)
            {
                _retryTimer = 0f;
                OnReachedCenter();
            }
            return;
        }

        MoveVisual();
    }

    private void MoveVisual()
    {
        if (_currentVisual == null) return;

        _currentVisual.transform.position = Vector3.MoveTowards(
            _currentVisual.transform.position,
            _targetPos,
            _moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(_currentVisual.transform.position, _targetPos) < 0.01f)
        {
            if (_phase == ItemPhase.ToCenter)
                OnReachedCenter();
            else if (_phase == ItemPhase.ToExit)
                OnReachedExit();
        }
    }

    private void OnReachedCenter()
    {
        if (_grid == null) return;

        int attempts = 0;
        while (attempts < 4)
        {
            _lastOutputIndex = (_lastOutputIndex + 1) % 4;
            Vector3 worldDir = transform.TransformDirection(_directions[_lastOutputIndex]);
            Vector3 neighborPos = transform.position + (worldDir * GridStepSize);
            IItemHolder neighbor = _grid.GetHolderAt(neighborPos);

            if (neighbor != null && IsOutputValid(neighbor, worldDir))
            {
                _targetPos = neighborPos + Vector3.up * 0.5f;
                _exitNeighbor = neighbor;
                _exitDir = worldDir;
                _phase = ItemPhase.ToExit;
                return;
            }
            attempts++;
        }

        // Alle Ausgänge voll - Retry
        _phase = ItemPhase.None;
        _retryTimer = 0f;
    }

    private void OnReachedExit()
    {
        if (_exitNeighbor != null && _exitNeighbor.TryAcceptItem(_currentItem, _currentVisual))
        {
            _currentItem = null;
            _currentVisual = null;
            _phase = ItemPhase.None;
        }
        else
        {
            // Nachbar doch voll - zurück zur Mitte
            _targetPos = transform.TransformPoint(_centerLocal);
            _phase = ItemPhase.ToCenter;
        }
    }

    private bool IsOutputValid(IItemHolder holder, Vector3 directionToNeighbor)
    {
        MonoBehaviour mb = holder as MonoBehaviour;
        if (mb != null)
        {
            if (Vector3.Dot(mb.transform.forward, directionToNeighbor) < -0.9f) return false;
        }
        return true;
    }

    public override bool TryAcceptItem(ItemDefinition item, GameObject visualObj)
    {
        if (_currentItem != null) return false;

        _currentItem = item;

        if (visualObj != null)
        {
            _currentVisual = visualObj;
            _currentVisual.transform.SetParent(transform);
        }
        else if (item.VisualPrefab != null)
        {
            _currentVisual = Instantiate(item.VisualPrefab, transform);
        }

        _targetPos = transform.TransformPoint(_centerLocal);
        _phase = ItemPhase.ToCenter;
        return true;
    }

    public override bool TryTakeItem(WorldItem item) => false;

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