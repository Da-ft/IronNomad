using UnityEngine;

public class HotbarSystem : MonoBehaviour
{
    public static HotbarSystem Instance { get; private set; }

    [Header("Tools")]
    [SerializeField] private BuilderTool _builderTool;

    private readonly BuildingDefinition[] _slots = new BuildingDefinition[9];

    public event System.Action<int, BuildingDefinition> OnSlotChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnEnable() => InputEvents.OnHotbarSelect += OnHotbarSelect;
    private void OnDisable() => InputEvents.OnHotbarSelect -= OnHotbarSelect;

    public void AssignSlot(int index, BuildingDefinition building)
    {
        if (index < 0 || index >= 9) return;
        _slots[index] = building;
        OnSlotChanged?.Invoke(index, building);
    }

    public BuildingDefinition GetSlot(int index)
        => (index >= 0 && index < 9) ? _slots[index] : null;

    private void OnHotbarSelect(int index)
    {
        BuildingDefinition def = GetSlot(index);
        if (def == null) return;
        _builderTool.SelectBuilding(def);
        Debug.Log($"[Hotbar] Slot {index + 1}: {def.DisplayName}");
    }
}