using UnityEngine;

public class MiningTool : BaseTool
{
    [Header("Dependencies")]
    [SerializeField] private InventorySystem _inventory;

    [Header("Config")]
    [SerializeField] private float _raycastRange = 5f;
    [SerializeField] private float _miningDamage = 25f;
    [SerializeField] private LayerMask _mineableLayer;

    private IMineable _currentTarget;

    public override void OnEquip() => InputEvents.OnPlace += TryMine;
    public override void OnUnequip() { InputEvents.OnPlace -= TryMine; _currentTarget = null; }

    private void Update() => UpdateTarget();

    private void UpdateTarget()
    {
        if (Physics.Raycast(GetCameraRay(), out RaycastHit hit, _raycastRange, _mineableLayer))
            _currentTarget = hit.collider.GetComponentInParent<IMineable>();
        else
            _currentTarget = null;
    }

    private void TryMine()
    {
        if (_currentTarget == null) return;
        _currentTarget.Mine(_miningDamage);
    }
}