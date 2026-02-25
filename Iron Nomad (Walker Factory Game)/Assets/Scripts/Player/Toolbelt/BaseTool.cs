using UnityEngine;

public abstract class BaseTool : MonoBehaviour
{
    public ToolDefinition Definition { get; private set; }

    public void Initialize(ToolDefinition definition)
    {
        Definition = definition;
    }

    // Call wenn Werkzeug ausgewählt wird
    public virtual void OnEquip() { }

    // Call wenn Werkzeug abgewählt wird
    public virtual void OnUnequip() { }
}
