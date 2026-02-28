using UnityEngine;

public enum ToolType { Build, Demolish, Mining }

[CreateAssetMenu(menuName = "IronNomad/Tool Definition")]
public class ToolDefinition : ScriptableObject
{
    public string DisplayName;
    public Sprite Icon;
    public ToolType Type;
}