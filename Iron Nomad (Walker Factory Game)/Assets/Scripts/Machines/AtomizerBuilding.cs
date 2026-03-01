using UnityEngine;

public class AtomizerBuilding : MonoBehaviour, IInteractable
{
    [SerializeField] private AtomizerMenuUI _menu;
    [SerializeField] private InventoryMenuUI _inventoryMenu;

    public string GetInteractPrompt() => "[E] Atomizer öffnen";

    public void OnInteract(InventorySystem inventory)
    {
        _menu.SetInventory(inventory);
        UIManager.Instance.OpenMenu(_menu);
        UIManager.Instance.OpenMenuAdditive(_inventoryMenu);
    }
}