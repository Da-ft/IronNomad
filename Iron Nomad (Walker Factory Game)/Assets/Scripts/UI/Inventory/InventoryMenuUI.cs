using IronNomad.Inputs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMenuUI : MonoBehaviour, IMenu
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private InventorySystem _inventory;

    [Header("UI References")]
    [SerializeField] private GameObject _menuRoot;
    [SerializeField] private Transform _inventoryContainer;
    [SerializeField] private Button _sortButton;

    [Header("Prefabs")]
    [SerializeField] private GameObject _slotPrefab;

    public bool IsOpen => _isOpen;
    private bool _isOpen = false;

    private List<SlotUI> _slots = new List<SlotUI>();

    private void OnEnable()
    {
        _inputReader.InventoryEvent += ToggleInventory;
        _inventory.OnInventoryChanged += Refresh;
    }

    private void OnDisable()
    {
        _inputReader.InventoryEvent -= ToggleInventory;
        _inventory.OnInventoryChanged -= Refresh;
    }

    private void Start()
    {
        UIManager.Instance.RegisterMenu(this);
        _menuRoot.SetActive(false);
        BuildSlots();

        if (_sortButton != null)
            _sortButton.onClick.AddListener(_inventory.SortByType);
    }

    private void OnDestroy()
    {
        UIManager.Instance?.UnregisterMenu(this);
    }

    // --- IMenu ---

    public void Open()
    {
        _isOpen = true;
        _menuRoot.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _inputReader.DisableGameplay();
        _inputReader.ResetLook();
        _inputReader.ResetMove();
        Refresh();
    }

    // Close macht nur das UI zu – Cursor/Gameplay regelt UIManager.CloseAll()
    public void Close()
    {
        _isOpen = false;
        _menuRoot.SetActive(false);
    }

    // --- Slots ---

    private void BuildSlots()
    {
        for (int i = 0; i < _inventory.TotalSlots; i++)
        {
            GameObject obj = Instantiate(_slotPrefab, _inventoryContainer);
            SlotUI slot = obj.GetComponent<SlotUI>();
            slot.Setup(i, _inventory);
            _slots.Add(slot);
        }
    }

    private void Refresh()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            InventorySlot data = _inventory.GetSlotAt(i);
            if (data != null)
                _slots[i].UpdateSlot(data, false);
        }
    }

    private void ToggleInventory()
    {
        if (_isOpen) UIManager.Instance.CloseAll();
        else UIManager.Instance.OpenMenu(this);
    }
}