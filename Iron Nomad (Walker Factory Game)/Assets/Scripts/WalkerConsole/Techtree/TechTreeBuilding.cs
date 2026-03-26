using UnityEngine;

public class TechTreeBuilding : MonoBehaviour, IInteractable
{
    [SerializeField] private WalkerConsoleUI _console;

    public string GetInteractPrompt() => "[E] Konsole öffnen";

    public void OnInteract(InventorySystem inventory)
    {
        UIManager.Instance.OpenMenu(_console);
    }
}