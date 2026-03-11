using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TechTreeSystem : MonoBehaviour
{
    public static TechTreeSystem Instance { get; private set; }

    private HashSet<ItemDefinition> _discoveredItems = new HashSet<ItemDefinition>();
    private HashSet<BuildingDefinition> _unlockedBuildings = new HashSet<BuildingDefinition>();

    public event System.Action<ItemDefinition> OnItemDiscovered;
    public event System.Action<BuildingDefinition> OnBuildingUnlocked;

    [SerializeField] private List<BuildingDefinition> _allBuildings;

    private void Awake()
    {
        _allBuildings = new List<BuildingDefinition>(Resources.LoadAll<BuildingDefinition>("Buildings"));

        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        foreach (var building in _allBuildings)
            if (building.IsUnlockedByDefault)
                _unlockedBuildings.Add(building);
    }

    // --- Items ---

    public bool IsDiscovered(ItemDefinition item) => _discoveredItems.Contains(item);

    public bool Discover(ItemDefinition item)
    {
        if (item == null || _discoveredItems.Contains(item)) return false;

        _discoveredItems.Add(item);
        OnItemDiscovered?.Invoke(item);
        Debug.Log($"[TechTree] Item '{item.Name}' entdeckt!");

        CheckAllBuildingUnlocks();
        return true;
    }

    // --- Buildings ---

    public bool IsBuildingUnlocked(BuildingDefinition building) => _unlockedBuildings.Contains(building);

    public int GetDiscoveredItemRequirementCount(BuildingDefinition building)
    {
        int count = 0;
        foreach (var item in building.ItemRequirements)
            if (_discoveredItems.Contains(item)) count++;
        return count;
    }

    public int GetUnlockedBuildingRequirementCount(BuildingDefinition building)
    {
        int count = 0;
        foreach (var req in building.BuildingRequirements)
            if (_unlockedBuildings.Contains(req)) count++;
        return count;
    }

    // Prüft nach jeder Änderung ob neue Gebäude freigeschaltet werden können
    private void CheckAllBuildingUnlocks()
    {
        bool anyNewUnlock = true;

        // Wiederholen bis keine neuen Unlocks mehr (Kettenreaktionen möglich)
        while (anyNewUnlock)
        {
            anyNewUnlock = false;
            foreach (var building in _allBuildings)
            {
                if (_unlockedBuildings.Contains(building)) continue;
                if (AreBuildingRequirementsMet(building))
                {
                    _unlockedBuildings.Add(building);
                    OnBuildingUnlocked?.Invoke(building);
                    Debug.Log($"[TechTree] '{building.DisplayName}' freigeschaltet!");
                    anyNewUnlock = true;
                }
            }
        }
    }

    private bool AreBuildingRequirementsMet(BuildingDefinition building)
    {
        foreach (var item in building.ItemRequirements)
            if (!_discoveredItems.Contains(item)) return false;

        foreach (var req in building.BuildingRequirements)
            if (!_unlockedBuildings.Contains(req)) return false;

        return true;
    }
}