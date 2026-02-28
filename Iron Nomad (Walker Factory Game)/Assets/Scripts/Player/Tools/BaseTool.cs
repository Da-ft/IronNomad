using UnityEngine;

public abstract class BaseTool : MonoBehaviour
{
    public ToolDefinition Definition { get; private set; }

    [Header("Base Dependencies")]
    [SerializeField] protected Transform _cameraRoot;

    public void Initialize(ToolDefinition definition)
    {
        Definition = definition;
    }

    public virtual void OnEquip() { }
    public virtual void OnUnequip() { }

    // Zentraler Ray-Helper – alle Tools nutzen denselben Ursprung
    protected Ray GetCameraRay() => new Ray(_cameraRoot.position, _cameraRoot.forward);
}