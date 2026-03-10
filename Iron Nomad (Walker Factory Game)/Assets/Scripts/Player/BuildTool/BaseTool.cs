using UnityEngine;

public abstract class BaseTool : MonoBehaviour
{
    [Header("Base Dependencies")]
    [SerializeField] protected Transform _cameraRoot;

    public virtual void OnEquip() { }
    public virtual void OnUnequip() { }

    protected Ray GetCameraRay() => new Ray(_cameraRoot.position, _cameraRoot.forward);
}