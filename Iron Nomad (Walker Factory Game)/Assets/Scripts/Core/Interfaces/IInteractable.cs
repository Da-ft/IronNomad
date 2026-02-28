public interface IInteractable
{
    string GetInteractPrompt();
    void OnInteract(InventorySystem inventory);
}