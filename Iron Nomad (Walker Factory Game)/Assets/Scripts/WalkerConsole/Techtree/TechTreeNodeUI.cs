using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TechTreeNodeUI : MonoBehaviour
{
    public enum NodeState { Hidden, Partial, Unlocked }

    [Header("Panels")]
    [SerializeField] private GameObject _hiddenPanel;    // Komplett verdeckt - "?"
    [SerializeField] private GameObject _partialPanel;   // Teilweise aufgedeckt
    [SerializeField] private GameObject _unlockedPanel;  // Freigeschaltet

    [Header("Partial Panel Refs")]
    [SerializeField] private TextMeshProUGUI _partialNameText;
    [SerializeField] private Transform _partialRequirementsContainer;
    [SerializeField] private GameObject _requirementEntryPrefab; // Label + Icon

    [Header("Unlocked Panel Refs")]
    [SerializeField] private TextMeshProUGUI _unlockedNameText;
    [SerializeField] private Image _unlockedIcon;

    public BuildingDefinition Building { get; private set; }
    public NodeState CurrentState { get; private set; } = NodeState.Hidden;

    public void Setup(BuildingDefinition building)
    {
        Building = building;
        Refresh();

        TechTreeSystem.Instance.OnItemDiscovered += _ => Refresh();
        TechTreeSystem.Instance.OnBuildingUnlocked += _ => Refresh();
    }

    private void OnDestroy()
    {
        if (TechTreeSystem.Instance == null) return;
        TechTreeSystem.Instance.OnItemDiscovered -= _ => Refresh();
        TechTreeSystem.Instance.OnBuildingUnlocked -= _ => Refresh();
    }

    public void Refresh()
    {
        if (TechTreeSystem.Instance.IsBuildingUnlocked(Building))
            SetState(NodeState.Unlocked);
        else if (HasAnyDiscoveredRequirement())
            SetState(NodeState.Partial);
        else
            SetState(NodeState.Hidden);
    }

    private void SetState(NodeState state)
    {
        CurrentState = state;

        _hiddenPanel.SetActive(state == NodeState.Hidden);
        _partialPanel.SetActive(state == NodeState.Partial);
        _unlockedPanel.SetActive(state == NodeState.Unlocked);

        if (state == NodeState.Partial)
            BuildPartialView();

        if (state == NodeState.Unlocked)
            BuildUnlockedView();
    }

    private void BuildPartialView()
    {
        _partialNameText.text = Building.DisplayName;

        foreach (Transform child in _partialRequirementsContainer)
            Destroy(child.gameObject);

        // Item Requirements
        foreach (var item in Building.ItemRequirements)
        {
            bool discovered = TechTreeSystem.Instance.IsDiscovered(item);
            SpawnRequirementEntry(
                discovered ? item.Name : "???",
                discovered ? item.Icon : null
            );
        }

        // Building Requirements
        foreach (var req in Building.BuildingRequirements)
        {
            bool unlocked = TechTreeSystem.Instance.IsBuildingUnlocked(req);
            SpawnRequirementEntry(
                unlocked ? req.DisplayName : "???",
                unlocked ? req.Icon : null
            );
        }
    }

    private void BuildUnlockedView()
    {
        _unlockedNameText.text = Building.DisplayName;
        if (_unlockedIcon != null)
        {
            _unlockedIcon.sprite = Building.Icon;
            _unlockedIcon.enabled = Building.Icon != null;
        }
    }

    private void SpawnRequirementEntry(string label, Sprite icon)
    {
        if (_requirementEntryPrefab == null) return;

        GameObject obj = Instantiate(_requirementEntryPrefab, _partialRequirementsContainer);

        TextMeshProUGUI text = obj.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null) text.text = label;

        Image img = obj.GetComponentInChildren<Image>();
        if (img != null)
        {
            img.sprite = icon;
            img.enabled = icon != null;
        }
    }

    private bool HasAnyDiscoveredRequirement()
    {
        foreach (var item in Building.ItemRequirements)
            if (TechTreeSystem.Instance.IsDiscovered(item)) return true;

        foreach (var req in Building.BuildingRequirements)
            if (TechTreeSystem.Instance.IsBuildingUnlocked(req)) return true;

        return false;
    }
}