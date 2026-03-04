using UnityEngine;

public enum ItemType { Generic, Ore, Ingot, Component }

[CreateAssetMenu(menuName = "IronNomad/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public GameObject VisualPrefab;
    [TextArea] public string Description;
    public ItemType Type;
    public int MaxStackSize = 64; // NEU
}