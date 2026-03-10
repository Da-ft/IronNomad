using UnityEngine;

[System.Serializable]
public class BuildingCost
{
    public ItemDefinition Item;
    public int Amount;
}

[CreateAssetMenu(menuName = "IronNomad/Building Definition")]
public class BuildingDefinition : ScriptableObject
{
    [Header("Info")]
    public string DisplayName;
    public Sprite Icon;
    [TextArea] public string Description;

    [Header("Kategorie")]
    public BuildingCategory Category;

    [Header("Prefabs")]
    public GameObject BuildingPrefab;
    public Material GhostMaterial;

    [Header("Kosten")]
    public BuildingCost[] Costs;

    [Header("Tech Tree")]
    public bool IsUnlockedByDefault = false;
    public ItemDefinition[] ItemRequirements;
    public BuildingDefinition[] BuildingRequirements;

    [Header("Grid")]
    public Vector2Int GridSize = Vector2Int.one;

    [Header("Ports")]
    public BuildingPort[] Ports;

    [Header("Placement")]
    [Tooltip("Aktiviert Satisfactory-style Belt-Placement statt normaler Ghost-Platzierung")]
    public bool UsesBeltPlacement = false;

    // --- Hilfsmethoden ---

    public BuildingPort[] GetInputPorts()
        => System.Array.FindAll(Ports, p => p.Type == PortType.Input);

    public BuildingPort[] GetOutputPorts()
        => System.Array.FindAll(Ports, p => p.Type == PortType.Output);

    public BuildingPort GetPort(string portName)
        => System.Array.Find(Ports, p => p.Name == portName);
}