using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Popup das erscheint wenn man ein Obstacle rechtsklickt.
/// Zeigt Name, Reparaturkosten, und Bestätigen-Button.
/// </summary>
public class WalkerRepairPanelUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _root;
    [SerializeField] private TextMeshProUGUI _titleLabel;
    [SerializeField] private TextMeshProUGUI _costsLabel;
    [SerializeField] private Image _icon;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    private System.Action _onConfirm;
    private WalkerObstacle _currentObstacle;
    private InventorySystem _inventory;

    private void Start()
    {
        _confirmButton.onClick.AddListener(OnConfirm);
        _cancelButton.onClick.AddListener(Hide);
        _root.SetActive(false);
    }

    public void Show(WalkerObstacle obstacle, InventorySystem inventory, System.Action onConfirm)
    {
        _currentObstacle = obstacle;
        _inventory = inventory;
        _onConfirm = onConfirm;

        _titleLabel.text = $"Reparieren: {obstacle.DisplayName}";

        if (_icon != null)
        {
            _icon.sprite = obstacle.Icon;
            _icon.enabled = obstacle.Icon != null;
        }

        // Kosten anzeigen
        var sb = new System.Text.StringBuilder("Kosten:\n");
        bool canAfford = true;
        if (obstacle.RepairCosts != null)
        {
            foreach (var cost in obstacle.RepairCosts)
            {
                int has = _inventory?.GetItemCount(cost.Item) ?? 0;
                bool enough = has >= cost.Amount;
                if (!enough) canAfford = false;
                sb.AppendLine($"• {cost.Amount}x {cost.Item.Name} ({has} vorhanden)");
            }
        }

        _costsLabel.text = sb.ToString();
        _confirmButton.interactable = canAfford;
        _root.SetActive(true);
    }

    public void Hide() => _root.SetActive(false);

    private void OnConfirm()
    {
        if (_currentObstacle?.RepairCosts != null)
            foreach (var cost in _currentObstacle.RepairCosts)
                _inventory?.RemoveItem(cost.Item, cost.Amount);

        _onConfirm?.Invoke();
        Hide();
    }
}