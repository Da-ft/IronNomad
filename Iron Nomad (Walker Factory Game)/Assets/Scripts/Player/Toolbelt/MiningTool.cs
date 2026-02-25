using IronNomad.Inputs;
using UnityEngine;

public class MiningTool : BaseTool
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _cameraRoot;
    [SerializeField] private InventorySystem _inventory;

    [Header("Config")]
    [SerializeField] private float _raycastRange = 5f;
    [SerializeField] private float _miningDamage = 25f;
    [SerializeField] private LayerMask _mineableLayer;

    // State
    private IMineable _currentTarget;
    private GameObject _currentTargetObj;

    // --- BaseTool ---

    public override void OnEquip()
    {
        _inputReader.PlaceEvent += TryMine;
    }

    public override void OnUnequip()
    {
        _inputReader.PlaceEvent -= TryMine;
        _currentTarget = null;
        _currentTargetObj = null;
    }

    private void Update()
    {
        UpdateTarget();
    }

    // --- Target Detection ---

    private void UpdateTarget()
    {
        Ray ray = new Ray(_cameraRoot.position, _cameraRoot.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, _raycastRange, _mineableLayer))
        {
            IMineable mineable = hit.collider.GetComponentInParent<IMineable>();
            if (mineable != null)
            {
                _currentTarget = mineable;
                _currentTargetObj = hit.collider.gameObject;
                return;
            }
        }

        _currentTarget = null;
        _currentTargetObj = null;
    }

    // --- Mining ---

    private void TryMine()
    {
        if (_currentTarget == null) return;

        _currentTarget.Mine(_miningDamage);
        Debug.Log($"Abgebaut! HP: {_currentTarget.CurrentHealth}/{_currentTarget.MaxHealth}");
    }
}