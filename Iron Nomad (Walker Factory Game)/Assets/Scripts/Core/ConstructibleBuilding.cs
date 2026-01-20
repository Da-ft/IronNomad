using UnityEngine;

public class ConstructibleBuilding : MonoBehaviour, IConstructible
{
    [SerializeField] private ItemDefinition _refundItem;

    public ItemDefinition GetRefundResource()
    {
        return _refundItem;
    }

    public void Demolish()
    {
        // TODO: Play FX for demolish
        Debug.Log($" Building {gameObject.name} abgerissen!");

        Destroy(gameObject);
    }
}
