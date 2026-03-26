using IronNomad.Inputs;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AtomizerMenuUI : MonoBehaviour, IMenu
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;

    [Header("UI References")]
    [SerializeField] private GameObject _menuRoot;
    [SerializeField] private AtomizerSlotUI _itemSlot;
    [SerializeField] private Button _atomizeButton;
    [SerializeField] private TextMeshProUGUI _feedbackText;

    public bool IsOpen => _isOpen;
    private bool _isOpen = false;
    private InventorySystem _inventory;

    private void Start()
    {
        UIManager.Instance.RegisterMenu(this);
        _menuRoot.SetActive(false);
        _atomizeButton.onClick.AddListener(TryAtomize);
    }

    private void OnDestroy()
    {
        UIManager.Instance?.UnregisterMenu(this);
    }

    public void SetInventory(InventorySystem inventory)
    {
        _inventory = inventory;
        _itemSlot.Setup(inventory);
    }

    // --- IMenu ---

    public void Open()
    {
        _isOpen = true;
        _menuRoot.SetActive(true);
        SetFeedback("");
    }

    // Close macht nur das UI zu – gibt Item zurück ins Inventar
    public void Close()
    {
        _isOpen = false;
        _itemSlot.ReturnItemToInventory();
        _menuRoot.SetActive(false);
    }

    // --- Atomize ---

    private void TryAtomize()
    {
        ItemDefinition item = _itemSlot.GetItem();

        if (item == null)
        {
            SetFeedback("Kein Item eingelegt.");
            return;
        }

        if (TechTreeSystem.Instance.IsDiscovered(item))
        {
            SetFeedback($"{item.Name} ist bereits bekannt.");
            return;
        }

        _itemSlot.ClearSlot();
        TechTreeSystem.Instance.Discover(item);
        SetFeedback($"{item.Name} atomisiert!");
    }

    private void SetFeedback(string message)
    {
        if (_feedbackText != null)
            _feedbackText.text = message;
    }
}