using System.Collections.Generic;
using UnityEngine;

public class Smelter : BaseGridMachine, IInteractable, IMachine
{
    [Header("Recipes")]
    [SerializeField] private CraftingRecipe[] _recipes;

    [Header("References")]
    [SerializeField] private Transform _inputPoint;
    [SerializeField] private Transform _outputPoint;

    private CraftingRecipe _currentRecipe;
    private Dictionary<ItemDefinition, int> _inputBuffer = new Dictionary<ItemDefinition, int>();
    private float _smeltTimer;
    private bool _isSmelting;

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        if (!_isSmelting) return;

        _smeltTimer += Time.deltaTime;

        if (_smeltTimer >= _currentRecipe.Duration)
        {
            _smeltTimer = 0f;
            ProduceOutput();
        }
    }

    private void ProduceOutput()
    {
        if (_grid == null || _currentRecipe == null) return;

        Vector3 targetPos = transform.position + (transform.forward * _grid.CellSize);
        IItemHolder nextHolder = _grid.GetHolderAt(targetPos);

        GameObject visual = null;
        if (_currentRecipe.Output.VisualPrefab != null)
        {
            Vector3 spawnPos = _outputPoint != null ? _outputPoint.position : transform.position;
            visual = Instantiate(_currentRecipe.Output.VisualPrefab, spawnPos, Quaternion.identity);
        }

        if (nextHolder == null || !nextHolder.TryAcceptItem(_currentRecipe.Output, visual))
        {
            if (visual != null) Destroy(visual);
            _smeltTimer = _currentRecipe.Duration; // Nächsten Frame nochmal versuchen
            return;
        }

        // Input verbrauchen
        for (int i = 0; i < _currentRecipe.Inputs.Length; i++)
        {
            ItemDefinition input = _currentRecipe.Inputs[i];
            int amount = i < _currentRecipe.InputAmounts.Length ? _currentRecipe.InputAmounts[i] : 1;

            _inputBuffer[input] -= amount;
            if (_inputBuffer[input] <= 0)
                _inputBuffer.Remove(input);
        }

        // Kann nochmal gecraftet werden?
        if (_currentRecipe.CanCraft(_inputBuffer))
        {
            _isSmelting = true;
        }
        else
        {
            _currentRecipe = null;
            _isSmelting = false;
        }
    }

    public override bool TryAcceptItem(ItemDefinition itemDef, GameObject existingVisual = null)
    {
        // Input-Richtung prüfen
        if (existingVisual != null && _inputPoint != null)
        {
            Vector3 itemDir = (existingVisual.transform.position - transform.position).normalized;
            Vector3 inputDir = (_inputPoint.position - transform.position).normalized;
            if (Vector3.Dot(itemDir, inputDir) < 0.5f) return false;
        }

        // Passt dieses Item zu irgendeinem Rezept?
        CraftingRecipe matchingRecipe = System.Array.Find(_recipes, r =>
            System.Array.Exists(r.Inputs, input => input == itemDef));

        if (matchingRecipe == null) return false;

        // Wenn bereits ein anderes Rezept läuft, nur Items akzeptieren die dazu passen
        if (_currentRecipe != null && _currentRecipe != matchingRecipe) return false;

        if (existingVisual != null) Destroy(existingVisual);

        // Input Buffer füllen
        if (!_inputBuffer.ContainsKey(itemDef))
            _inputBuffer[itemDef] = 0;
        _inputBuffer[itemDef]++;

        // Starten sobald genug da ist
        if (matchingRecipe.CanCraft(_inputBuffer))
        {
            _currentRecipe = matchingRecipe;
            _isSmelting = true;
        }

        return true;
    }

    public override bool TryTakeItem(WorldItem item) => false;

    public CraftingRecipe[] GetRecipes() => _recipes;
    public CraftingRecipe GetCurrentRecipe() => _currentRecipe;
    public void SetRecipe(CraftingRecipe recipe)
    {
        _currentRecipe = recipe;
        _inputBuffer.Clear();
        _isSmelting = false;
        _smeltTimer = 0f;
    }

    public string GetInteractPrompt() => "Smelter öffnen";
    public void OnInteract(InventorySystem inventory)
    {
        MachineMenuUI.Instance.Open(this);
    }
}