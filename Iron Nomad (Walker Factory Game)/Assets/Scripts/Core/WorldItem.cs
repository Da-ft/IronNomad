using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WorldItem : MonoBehaviour, IInteractable
{
    public ItemDefinition Definition { get; private set; }
    private IItemHolder _currentHolder;

    // Wird vom Container aufgerufen wenn das item spawned
    public void Initialize(ItemDefinition def, IItemHolder holder)
    {
        Definition = def;
        _currentHolder = holder;

        // Object auf "Interactable" Layer
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    // Call durch Player Interaction
    public ItemDefinition PickUp()
    {
        if (_currentHolder == null) return null;

        // Frag besitzer (Schacht/Conveyor Belt) ob man gehen darf
        if (_currentHolder.TryTakeItem(this))
        {
            // Ja wir dürfen, zerstöre gameobject, Daten zurückgeben!
            Destroy(gameObject);
            return Definition;
        }

        return null;
    }

    public string GetInteractPrompt() => $"Nimm {Definition.Name}";

    public void OnInteract()
    {
        PickUp();
    }
}
