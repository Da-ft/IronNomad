using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StackSplitMenu : MonoBehaviour
{
    public static StackSplitMenu Instance { get; private set; }

    [SerializeField] private GameObject _menuRoot;
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _amountText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    private int _sourceSlotIndex;
    private InventorySystem _inventory;
    private ItemDefinition _item;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _menuRoot.SetActive(false);
    }

    private void Start()
    {
        _slider.onValueChanged.AddListener(OnSliderChanged);
        _confirmButton.onClick.AddListener(Confirm);
        _cancelButton.onClick.AddListener(Cancel);
    }

    public void Open(int slotIndex, InventorySlot slot, InventorySystem inventory)
    {
        _sourceSlotIndex = slotIndex;
        _inventory = inventory;
        _item = slot.Item;

        _slider.minValue = 1;
        _slider.maxValue = slot.Count;
        _slider.value = Mathf.FloorToInt(slot.Count / 2f);

        _amountText.text = ((int)_slider.value).ToString();
        _menuRoot.SetActive(true);

        // Menü neben dem Mauszeiger positionieren
        _menuRoot.transform.position = Input.mousePosition + new Vector3(10, -10, 0);
    }

    private void OnSliderChanged(float value)
    {
        _amountText.text = ((int)value).ToString();
    }

    private void Confirm()
    {
        int amount = (int)_slider.value;

        if (_inventory.TakeFromStack(_sourceSlotIndex, amount))
        {
            // Am Mauszeiger halten via DragDropManager
            DragDropManager.Instance.BeginDragWithItem(_item, amount);
        }

        _menuRoot.SetActive(false);
    }

    private void Cancel()
    {
        _menuRoot.SetActive(false);
    }
}