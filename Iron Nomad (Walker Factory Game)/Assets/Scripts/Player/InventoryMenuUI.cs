using IronNomad.Inputs;
using System.Collections.Generic;
using UnityEngine;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public class InventoryMenuUI : MonoBehaviour, IMenu
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private InventorySystem _inventory;

    [Header("UI References")]
    [SerializeField] private GameObject _menuRoot;
    [SerializeField] private Transform _hotbarContainer;  // HLG_Hotbar
    [SerializeField] private Transform _inventoryContainer; // GLG_InventoryGrid

    [Header("Prefabs")]
    [SerializeField] private GameObject _slotPrefab;

    public bool IsOpen => _isOpen;
    private bool _isOpen = false;

    private List<HotbarSlotUI> _allSlots = new List<HotbarSlotUI>();
    private int _selectedIndex = 0;

    private void OnEnable()
    {
        _inputReader.InventoryEvent += OpenInventory;  
        _inputReader.CloseMenuEvent += TryClose;     
    }
    private void OnDisable()
    {
        _inputReader.InventoryEvent -= OpenInventory;
        _inputReader.CloseMenuEvent -= TryClose;
    }

    private void Start()
    {
        UIManager.Instance.RegisterMenu(this);
        _menuRoot.SetActive(false);
        BuildSlots();
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

    public void Close()
    {
        _isOpen = false;
        _menuRoot.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _inputReader.EnableGameplay();
    }


    // --- Slots ---

    private void BuildSlots()
    {
        // Hotbar Slots (oben im Inventar)
        for (int i = 0; i < _inventory.HotbarSize; i++)
        {
            GameObject obj = Instantiate(_slotPrefab, _hotbarContainer);
            _allSlots.Add(obj.GetComponent<HotbarSlotUI>());
        }

        // Rest der Slots
        for (int i = _inventory.HotbarSize; i < _inventory.TotalSlots; i++)
        {
            GameObject obj = Instantiate(_slotPrefab, _inventoryContainer);
            _allSlots.Add(obj.GetComponent<HotbarSlotUI>());
        }
    }

    private void Refresh()
    {
        for (int i = 0; i < _allSlots.Count; i++)
        {
            InventorySlot data = _inventory.GetSlotAt(i);
            if (data != null)
                _allSlots[i].UpdateSlot(data, i == _selectedIndex);
        }
    }

    private void OpenInventory()
    {
        if (_isOpen)
            Close();
        else
            UIManager.Instance.OpenMenu(this);
    }

    private void TryClose()
    {
        if (_isOpen) Close();
    }
}