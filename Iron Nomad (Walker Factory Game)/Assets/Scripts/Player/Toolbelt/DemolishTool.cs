using IronNomad.Inputs;
using System.Collections.Generic;
using UnityEngine;

public class DemolishTool : BaseTool
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _cameraRoot;
    [SerializeField] private WalkerGrid _grid;
    [SerializeField] private InventorySystem _inventory;

    [Header("Config")]
    [SerializeField] private float _raycastRange = 10f;
    [SerializeField] private Material _highlightMaterial;
    [SerializeField] private LayerMask _demolishLayer;

    // State
    private GameObject _demolishTarget;
    private List<Material[]> _originalMaterials = new List<Material[]>();

    // --- BaseTool ---

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

    // --- Preview ---

    private void UpdatePreview()
    {
        Ray ray = new Ray(_cameraRoot.position, _cameraRoot.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, _raycastRange, _demolishLayer))
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

        foreach (var renderer in target.GetComponentsInChildren<Renderer>())
        {
            _originalMaterials.Add(renderer.materials);

            Material[] redMats = new Material[renderer.materials.Length];
            for (int i = 0; i < redMats.Length; i++)
                redMats[i] = _highlightMaterial;
            renderer.materials = redMats;
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

    // --- Demolish ---

    private void TryDemolishConfirm()
    {
        if (_demolishTarget == null) return;

        ConstructibleBuilding building = _demolishTarget.GetComponent<ConstructibleBuilding>();
        if (building == null) return;

        // Ressourcen zurückgeben
        if (building.Definition != null && _inventory != null)
        {
            foreach (var cost in building.Definition.Costs)
            {
                _inventory.AddItem(cost.Item, cost.Amount);
                Debug.Log($"Refund: {cost.Amount}x {cost.Item.name}");
            }
        }

        _grid.FreeCell(building.transform.position);
        ClearHighlight();
        building.Demolish();
    }
}