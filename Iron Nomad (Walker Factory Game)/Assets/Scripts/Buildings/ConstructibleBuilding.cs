using UnityEngine;

public class ConstructibleBuilding : MonoBehaviour, IConstructible
{
    public BuildingDefinition Definition { get; private set; }
    public int Rotation { get; private set; }

    public void Initialize(BuildingDefinition definition, int rotation)
    {
        Definition = definition;
        Rotation = rotation;
    }

    public ItemDefinition GetRefundResource()
    {
        return Definition?.Costs.Length > 0 ? Definition.Costs[0].Item : null;
    }

    public void Demolish() => Destroy(gameObject);
}