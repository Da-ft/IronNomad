using System.Collections.Generic;
using UnityEngine;

public class Smelter : BaseGridMachine, IInteractable, IMachine
{
    [Header("Recipes")]
    [SerializeField] private CraftingRecipe[] _recipes;

    [Header("References")]
    [SerializeField] private Transform _outputPoint;

    [Header("Buffer")]
    [SerializeField] private int _inputBufferSize = 1;
    [SerializeField] private int _outputBufferSize = 1;

    private CraftingRecipe _currentRecipe;
    private Dictionary<ItemDefinition, int> _inputBuffer = new();
    private Dictionary<ItemDefinition, int> _outputBuffer = new();

    private float _smeltTimer;
    private bool _isSmelting;

    // --- Throughput Tracking ---
    private float _trackingWindow = 10f;
    private float _trackingTimer = 0f;
    private float _inputReceived = 0f;
    private float _itemsProcessed = 0f;
    private float _cachedInputPerMin = 0f;
    private float _cachedOutputPerMin = 0f;

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        if (_isSmelting)
        {
            _smeltTimer += Time.deltaTime;
            _trackingTimer += Time.deltaTime;

            if (_trackingTimer >= _trackingWindow)
            {
                _cachedInputPerMin = (_inputReceived / _trackingWindow) * 60f;
                _cachedOutputPerMin = (_itemsProcessed / _trackingWindow) * 60f;
                _inputReceived = 0f;
                _itemsProcessed = 0f;
                _trackingTimer = 0f;
            }

            if (_smeltTimer >= _currentRecipe.Duration)
            {
                _smeltTimer = 0f;
                ProduceOutput();
            }
        }

        if (_outputBuffer.Count > 0)
            TryPushOutput();
    }

    private void ProduceOutput()
    {
        if (_currentRecipe == null) return;

        ItemDefinition outputItem = _currentRecipe.Output;
        int maxOutput = _outputBufferSize * outputItem.MaxStackSize;

        if (!_outputBuffer.ContainsKey(outputItem))
            _outputBuffer[outputItem] = 0;

        // Output-Buffer voll → warten
        if (_outputBuffer[outputItem] >= maxOutput)
        {
            _smeltTimer = _currentRecipe.Duration;
            return;
        }

        _outputBuffer[outputItem] += _currentRecipe.OutputAmount;
        _itemsProcessed += _currentRecipe.OutputAmount;

        // Input verbrauchen
        for (int i = 0; i < _currentRecipe.Inputs.Length; i++)
        {
            ItemDefinition input = _currentRecipe.Inputs[i];
            int amount = i < _currentRecipe.InputAmounts.Length ? _currentRecipe.InputAmounts[i] : 1;
            _inputBuffer[input] -= amount;
            if (_inputBuffer[input] <= 0) _inputBuffer.Remove(input);
        }

        if (_currentRecipe.CanCraft(_inputBuffer))
            _isSmelting = true;
        else
        {
            _currentRecipe = null;
            _isSmelting = false;
        }

        TryPushOutput();
    }

    private void TryPushOutput()
    {
        if (_constructible?.Definition == null) return;

        // Alle Output-Ports durchgehen
        foreach (var port in _constructible.Definition.GetOutputPorts())
        {
            foreach (var kvp in new Dictionary<ItemDefinition, int>(_outputBuffer))
            {
                ItemDefinition item = kvp.Key;
                if (_outputBuffer[item] <= 0) continue;

                GameObject visual = null;
                if (item.VisualPrefab != null)
                {
                    Vector3 spawnPos = _outputPoint != null ? _outputPoint.position : transform.position;
                    visual = Instantiate(item.VisualPrefab, spawnPos, Quaternion.identity);
                }

                if (TryPushItemToPort(port, item, visual))
                {
                    _outputBuffer[item]--;
                    if (_outputBuffer[item] <= 0) _outputBuffer.Remove(item);
                }
                else
                {
                    if (visual != null) Destroy(visual);
                }
            }
        }
    }

    // --- IItemHolder ---

    public override bool TryAcceptItem(ItemDefinition itemDef, GameObject existingVisual = null)
    {
        // Herkunft prüfen: muss von einem Input-Port kommen
        if (existingVisual != null && !IsFromAnyInputPort(existingVisual.transform.position))
            return false;

        // Passendes Rezept finden
        CraftingRecipe match = System.Array.Find(_recipes, r =>
            System.Array.Exists(r.Inputs, input => input == itemDef));
        if (match == null) return false;

        // Kein Rezept-Konflikt
        if (_currentRecipe != null && _currentRecipe != match) return false;

        // Buffer-Check
        int maxBuffer = _inputBufferSize * itemDef.MaxStackSize;
        int currentBuffer = _inputBuffer.ContainsKey(itemDef) ? _inputBuffer[itemDef] : 0;
        if (currentBuffer >= maxBuffer) return false;

        if (existingVisual != null) Destroy(existingVisual);

        if (!_inputBuffer.ContainsKey(itemDef)) _inputBuffer[itemDef] = 0;
        _inputBuffer[itemDef]++;

        if (match.CanCraft(_inputBuffer))
        {
            _currentRecipe = match;
            _inputReceived += match.InputAmounts.Length > 0 ? match.InputAmounts[0] : 1;
            _isSmelting = true;
        }

        return true;
    }

    public override bool TryTakeItem(WorldItem item) => false;

    public override bool IsFull
    {
        get
        {
            if (_currentRecipe == null) return false;
            foreach (var input in _currentRecipe.Inputs)
            {
                int current = _inputBuffer.ContainsKey(input) ? _inputBuffer[input] : 0;
                if (current < _inputBufferSize * input.MaxStackSize) return false;
            }
            return true;
        }
    }

    // --- IMachine ---

    public CraftingRecipe[] GetRecipes() => _recipes;
    public CraftingRecipe GetCurrentRecipe() => _currentRecipe;
    public float GetProgress() => _currentRecipe != null ? _smeltTimer / _currentRecipe.Duration : 0f;
    public float GetInputPerMinute() => _cachedInputPerMin;
    public float GetOutputPerMinute() => _cachedOutputPerMin;

    public void SetRecipe(CraftingRecipe recipe)
    {
        _currentRecipe = recipe;
        _inputBuffer.Clear();
        _outputBuffer.Clear();
        _isSmelting = false;
        _smeltTimer = 0f;
    }

    // --- IInteractable ---

    public string GetInteractPrompt() => "Smelter öffnen";

    public void OnInteract(InventorySystem inventory)
    {
        MachineMenuUI.Instance.Open(this);
    }
}