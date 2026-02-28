using UnityEngine;

public class AtomizerBuilding : MonoBehaviour, IInteractable
{
    [SerializeField] private AtomizerMenuUI _menu;

    public string GetInteractPrompt() => "[E] Atomizer öffnen";

    public void OnInteract(InventorySystem inventory)
    {
        UIManager.Instance.OpenMenu(_menu);
        _menu.SetInventory(inventory);
    }
}