using UnityEngine;

[CreateAssetMenu(menuName = "IronNomad/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public GameObject VisualPrefab; // 3D Objekt für Conveyor Belt
    [TextArea] public string Description;
}
