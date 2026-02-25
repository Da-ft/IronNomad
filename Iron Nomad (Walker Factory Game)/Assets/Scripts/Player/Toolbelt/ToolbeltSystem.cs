using System.Collections.Generic;
using UnityEngine;
using IronNomad.Inputs;

public class ToolbeltSystem : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;

    [Header("Tools")]
    [SerializeField] private List<ToolDefinition> _tools;

    [Header("Tool References")]
    [SerializeField] private BuilderTool _builderTool;
    [SerializeField] private DemolishTool _demolishTool;
    [SerializeField] private MiningTool _miningTool;

    public int SelectedIndex { get; private set; } = 0;
    public BaseTool ActiveTool { get; private set; }
    public int ToolCount => _tools.Count;

    public event System.Action<int> OnSelectionChanged;

    private List<BaseTool> _toolInstances = new List<BaseTool>();

    private void Start()
    {
        InitializeTools();
        EquipTool(0);
    }

    private void OnEnable()
    {
        _inputReader.ScrollEvent += HandleScroll;
        _inputReader.HotbarSelectEvent += HandleHotbarKey;
    }

    private void OnDisable()
    {
        _inputReader.ScrollEvent -= HandleScroll;
        _inputReader.HotbarSelectEvent -= HandleHotbarKey;
    }

    private void InitializeTools()
    {
        foreach (var def in _tools)
        {
            BaseTool tool = GetToolInstance(def.Type);
            if (tool != null)
            {
                tool.Initialize(def);
                tool.enabled = false;
                _toolInstances.Add(tool);
            }
        }
    }

    private BaseTool GetToolInstance(ToolType type)
    {
        switch (type)
        {
            case ToolType.Build: return _builderTool;
            case ToolType.Demolish: return _demolishTool;
            case ToolType.Mining: return _miningTool;
            default: return null;
        }
    }

    public void EquipTool(int index)
    {
        if (index < 0 || index >= _toolInstances.Count) return;

        // Altes Tool deaktivieren
        if (ActiveTool != null)
        {
            ActiveTool.OnUnequip();
            ActiveTool.enabled = false;
        }

        // Neues Tool aktivieren
        SelectedIndex = index;
        ActiveTool = _toolInstances[index];
        ActiveTool.enabled = true;
        ActiveTool.OnEquip();

        OnSelectionChanged?.Invoke(SelectedIndex);
        Debug.Log($"Tool ausgerüstet: {ActiveTool.Definition.DisplayName}");
    }

    private void HandleScroll(float direction)
    {
        int newIndex = SelectedIndex + (direction > 0 ? -1 : 1);
        if (newIndex < 0) newIndex = _toolInstances.Count - 1;
        if (newIndex >= _toolInstances.Count) newIndex = 0;
        EquipTool(newIndex);
    }

    private void HandleHotbarKey(int index)
    {
        if (index < _toolInstances.Count)
            EquipTool(index);
    }

    public ToolDefinition GetToolAt(int index)
    {
        if (index < 0 || index >= _tools.Count) return null;
        return _tools[index];
    }
}