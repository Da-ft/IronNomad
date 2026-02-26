using UnityEngine;
using UnityEngine.UI;

public class SortButtonUI : MonoBehaviour
{
    [SerializeField] private InventorySystem _inventory;
    [SerializeField] private Button _button;

    private void Start()
    {
        _button.onClick.AddListener(_inventory.SortByType);
    }
}