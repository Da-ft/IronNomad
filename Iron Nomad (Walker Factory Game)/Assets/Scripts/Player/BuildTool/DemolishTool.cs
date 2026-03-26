using System.Collections.Generic;
using UnityEngine;

public class DemolishTool : BaseTool
{
    [Header("Dependencies")]
    [SerializeField] private WalkerGrid _grid;
    [SerializeField] private InventorySystem _inventory;

    [Header("Config")]
    [SerializeField] private float _raycastRange = 10f;
    [SerializeField] private Material _highlightMaterial;
    [SerializeField] private LayerMask _demolishLayer;

    private GameObject _demolishTarget;
    private List<Material[]> _originalMaterials = new();

    public override void OnEquip() => InputEvents.OnDemolishConfirm += TryDemolishConfirm;
    public override void OnUnequip() { InputEvents.OnDemolishConfirm -= TryDemolishConfirm; ClearHighlight(); }

    private void Update() => UpdatePreview();

    private void UpdatePreview()
    {
        if (Physics.Raycast(GetCameraRay(), out RaycastHit hit, _raycastRange, _demolishLayer))
        {
            ConstructibleBuilding target = hit.collider.GetComponentInParent<ConstructibleBuilding>();
            if (target != null && target.gameObject != _demolishTarget)
            {
                ClearHighlight();
                HighlightTarget(target.gameObject);
            }
        }
        else ClearHighlight();
    }

    private void HighlightTarget(GameObject target)
    {
        _demolishTarget = target;
        _originalMaterials.Clear();
        foreach (var r in target.GetComponentsInChildren<Renderer>())
        {
            _originalMaterials.Add(r.materials);
            var mats = new Material[r.materials.Length];
            for (int i = 0; i < mats.Length; i++) mats[i] = _highlightMaterial;
            r.materials = mats;
        }
    }

    private void ClearHighlight()
    {
        if (_demolishTarget == null) return;
        var renderers = _demolishTarget.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length && i < _originalMaterials.Count; i++)
            renderers[i].materials = _originalMaterials[i];
        _demolishTarget = null;
        _originalMaterials.Clear();
    }

    private void TryDemolishConfirm()
    {
        if (_demolishTarget == null) return;
        ConstructibleBuilding building = _demolishTarget.GetComponent<ConstructibleBuilding>();
        if (building == null) return;

        if (building.Definition != null && _inventory != null)
            foreach (var cost in building.Definition.Costs)
                _inventory.AddItem(cost.Item, cost.Amount);

        _grid.FreeCell(building.transform.position, building.Definition.GridSize, building.Rotation);
        ClearHighlight();
        building.Demolish();
    }
}