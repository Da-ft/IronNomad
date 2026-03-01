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
}