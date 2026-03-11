using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Zeigt Infos zur aktuell ausgewählten Zelle.
/// Wird von WalkerPowerMenuUI befüllt und gesteuert.
/// </summary>
public class WalkerSelectedPanelUI : MonoBehaviour
{
    [Header("Labels")]
    [SerializeField] private TextMeshProUGUI _nameLabel;
    [SerializeField] private TextMeshProUGUI _productionLabel;
    [SerializeField] private TextMeshProUGUI _costsLabel;

    [Header("Buttons")]
    [SerializeField] private Button _buildButton;
    [SerializeField] private Button _demolishButton;
    [SerializeField] private Button _repairButton;

    public event System.Action OnBuildClicked;
    public event System.Action OnDemolishClicked;
    public event System.Action OnRepairClicked;

    private void Start()
    {
        _buildButton.onClick.AddListener(() => OnBuildClicked?.Invoke());
        _demolishButton.onClick.AddListener(() => OnDemolishClicked?.Invoke());
        _repairButton.onClick.AddListener(() => OnRepairClicked?.Invoke());

        ShowEmpty();
    }

    /// <summary>Leere Zelle ausgewählt — kein Gebäude selektiert.</summary>
    public void ShowEmpty()
    {
        _nameLabel.text = "Leer";
        _productionLabel.text = "";
        _costsLabel.text = "";

        _buildButton.interactable = false;
        _demolishButton.interactable = false;
        _repairButton.gameObject.SetActive(false);
        _demolishButton.gameObject.SetActive(true);
    }

    /// <summary>Leere Zelle + Gebäude aus Liste ausgewählt → bereit zum Bauen.</summary>
    public void ShowBuildPreview(BuildingDefinition def, bool canAfford)
    {
        _nameLabel.text = def.DisplayName;
        _productionLabel.text = def.PowerProduction > 0 ? $"Produktion: {def.PowerProduction:F1} MW" : "";
        _costsLabel.text = FormatCosts("Baukosten", def.Costs);

        _buildButton.interactable = canAfford;
        _demolishButton.interactable = false;
        _repairButton.gameObject.SetActive(false);
        _demolishButton.gameObject.SetActive(true);
    }

    /// <summary>Belegte Zelle ausgewählt.</summary>
    public void ShowOccupied(BuildingDefinition def)
    {
        _nameLabel.text = def.DisplayName;
        _productionLabel.text = def.PowerProduction > 0 ? $"Produktion: {def.PowerProduction:F1} MW" : "";
        _costsLabel.text = FormatCosts("Rückgabe", def.Costs);

        _buildButton.interactable = false;
        _demolishButton.interactable = true;
        _repairButton.gameObject.SetActive(false);
        _demolishButton.gameObject.SetActive(true);
    }

    /// <summary>Obstacle ausgewählt.</summary>
    public void ShowObstacle(WalkerObstacle obs, bool canAfford)
    {
        _nameLabel.text = obs.DisplayName;
        _productionLabel.text = "";
        _costsLabel.text = FormatCosts("Reparaturkosten", obs.RepairCosts);

        _buildButton.interactable = false;
        _demolishButton.interactable = false;
        _repairButton.interactable = canAfford;
        _repairButton.gameObject.SetActive(true);
        _demolishButton.gameObject.SetActive(false);
    }

    // --- Helper ---

    private string FormatCosts(string header, BuildingCost[] costs)
    {
        if (costs == null || costs.Length == 0) return "";
        var sb = new System.Text.StringBuilder($"{header}:\n");
        foreach (var cost in costs)
            sb.AppendLine($"• {cost.Amount}x {cost.Item.Name}");
        return sb.ToString();
    }
}