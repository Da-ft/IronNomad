using IronNomad.Inputs;
using System.Collections.Generic;
using UnityEngine;

public class DemolishTool : BaseTool
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private WalkerGrid _grid;
    [SerializeField] private InventorySystem _inventory;

    [Header("Config")]
    [SerializeField] private float _raycastRange = 10f;
    [SerializeField] private Material _highlightMaterial;
    [SerializeField] private LayerMask _demolishLayer;

    private GameObject _demolishTarget;
    private List<Material[]> _originalMaterials = new List<Material[]>();

    public override void OnEquip()
    {
        _inputReader.DemolishConfirmEvent += TryDemolishConfirm;
    }

    public override void OnUnequip()
    {
        _inputReader.DemolishConfirmEvent -= TryDemolishConfirm;
        ClearHighlight();
    }

    private void Update()
    {
        UpdatePreview();
    }

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
        else
        {
            ClearHighlight();
        }
    }

    private void HighlightTarget(GameObject target)
    {
        _demolishTarget = target;
        _originalMaterials.Clear();

        foreach (var r in target.GetComponentsInChildren<Renderer>())
        {
            _originalMaterials.Add(r.materials);
            Material[] redMats = new Material[r.materials.Length];
            for (int i = 0; i < redMats.Length; i++)
                redMats[i] = _highlightMaterial;
            r.materials = redMats;
        }
    }

    private void ClearHighlight()
    {
        if (_demolishTarget == null) return;

        Renderer[] renderers = _demolishTarget.GetComponentsInChildren<Renderer>();
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
        {
            foreach (var cost in building.Definition.Costs)
                _inventory.AddItem(cost.Item, cost.Amount);
        }

        _grid.FreeCell(building.transform.position);
        ClearHighlight();
        building.Demolish();
    }
}