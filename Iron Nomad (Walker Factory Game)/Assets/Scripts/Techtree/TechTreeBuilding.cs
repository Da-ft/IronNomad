using UnityEngine;

public class TechTreeBuilding : MonoBehaviour, IInteractable
{
    [SerializeField] private TechTreeMenuUI _menu;

    public string GetInteractPrompt() => "[E] Techtree öffnen";

    public void OnInteract(InventorySystem inventory)
    {
        UIManager.Instance.OpenMenu(_menu);
    }
}