using UnityEngine;

[CreateAssetMenu(menuName = "IronNomad/Building Category")]
public class BuildingCategory : ScriptableObject
{
    public string DisplayName;
    public Sprite Icon;
    [TextArea] public string Description;
}
