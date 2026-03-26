using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MachineMenuUI : MonoBehaviour, IMenu
{
    public static MachineMenuUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject _menuRoot;
    [SerializeField] private TextMeshProUGUI _machineName;

    [Header("View: Recipe Selection")]
    [SerializeField] private GameObject _viewRecipeSelection;
    [SerializeField] private Transform _recipeList;
    [SerializeField] private GameObject _recipeInfoPanel;
    [SerializeField] private TextMeshProUGUI _recipeInputText;
    [SerializeField] private TextMeshProUGUI _recipeOutputText;
    [SerializeField] private TextMeshProUGUI _recipeDurationText;
    [SerializeField] private Button _btnSelectRecipe;

    [Header("View: Production")]
    [SerializeField] private GameObject _viewProduction;
    [SerializeField] private TextMeshProUGUI _activeRecipeText;
    [SerializeField] private TextMeshProUGUI _inputPerMinText;
    [SerializeField] private TextMeshProUGUI _outputPerMinText;
    [SerializeField] private Image _progressFill;
    [SerializeField] private Button _btnChangeRecipe;

    [Header("Prefabs")]
    [SerializeField] private GameObject _recipeCardPrefab; // SelectableCardUI

    public bool IsOpen => _isOpen;
    private bool _isOpen = false;
    private IMachine _currentMachine;
    private CraftingRecipe _highlightedRecipe;
    private SelectableCardUI _selectedCard;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void Start()
    {
        UIManager.Instance.RegisterMenu(this);
        _menuRoot.SetActive(false);
        _btnSelectRecipe.onClick.AddListener(OnSelectRecipeConfirmed);
        _btnChangeRecipe.onClick.AddListener(ShowRecipeSelection);
    }

    private void OnDestroy() => UIManager.Instance?.UnregisterMenu(this);

    private void Update()
    {
        if (!_isOpen || _currentMachine == null) return;
        if (!_viewProduction.activeSelf) return;

        _progressFill.fillAmount = _currentMachine.GetProgress();
        _inputPerMinText.text = $"Input: {_currentMachine.GetInputPerMinute():F1}/min";
        _outputPerMinText.text = $"Output: {_currentMachine.GetOutputPerMinute():F1}/min";
    }

    // --- IMenu ---

    public void Open(IMachine machine)
    {
        _currentMachine = machine;
        UIManager.Instance.OpenMenu(this);
    }

    public void Open()
    {
        _isOpen = true;
        _menuRoot.SetActive(true);

        if (_currentMachine.GetCurrentRecipe() == null) ShowRecipeSelection();
        else ShowProduction();
    }

    public void Close()
    {
        _isOpen = false;
        _currentMachine = null;
        _menuRoot.SetActive(false);
    }

    // --- Views ---

    private void ShowRecipeSelection()
    {
        _viewRecipeSelection.SetActive(true);
        _viewProduction.SetActive(false);
        _recipeInfoPanel.SetActive(false);
        _btnSelectRecipe.interactable = false;
        _highlightedRecipe = null;
        _selectedCard = null;
        BuildRecipeList();
    }

    private void ShowProduction()
    {
        _viewRecipeSelection.SetActive(false);
        _viewProduction.SetActive(true);

        CraftingRecipe active = _currentMachine.GetCurrentRecipe();
        if (active != null)
        {
            _activeRecipeText.text = active.Output.Name;
            _progressFill.sprite = active.Output.Icon;
            _progressFill.color = Color.white;
        }
    }

    // --- Recipe List ---

    private void BuildRecipeList()
    {
        foreach (Transform child in _recipeList) Destroy(child.gameObject);

        CraftingRecipe[] recipes = _currentMachine.GetRecipes();
        _machineName.text = recipes.Length > 0 ? "Rezept wählen" : "Keine Rezepte verfügbar";

        foreach (CraftingRecipe recipe in recipes)
        {
            var captured = recipe;
            GameObject obj = Instantiate(_recipeCardPrefab, _recipeList);
            SelectableCardUI card = obj.GetComponent<SelectableCardUI>();
            card.Setup(
                label: recipe.Output.Name,
                icon: recipe.Output.Icon,
                onClick: () => OnRecipeHighlighted(captured, card)
            );
        }
    }

    private void OnRecipeHighlighted(CraftingRecipe recipe, SelectableCardUI card)
    {
        _selectedCard?.SetSelected(false);
        _selectedCard = card;
        _selectedCard.SetSelected(true);

        _highlightedRecipe = recipe;
        _btnSelectRecipe.interactable = true;
        ShowRecipeInfo(recipe);
    }

    private void OnSelectRecipeConfirmed()
    {
        if (_highlightedRecipe == null) return;
        _currentMachine.SetRecipe(_highlightedRecipe);
        _highlightedRecipe = null;
        ShowProduction();
    }

    private void ShowRecipeInfo(CraftingRecipe recipe)
    {
        _recipeInfoPanel.SetActive(true);

        var inputs = new System.Text.StringBuilder();
        for (int i = 0; i < recipe.Inputs.Length; i++)
        {
            int amount = i < recipe.InputAmounts.Length ? recipe.InputAmounts[i] : 1;
            inputs.AppendLine($"• {amount}x {recipe.Inputs[i].Name}");
        }

        _recipeInputText.text = inputs.ToString();
        _recipeOutputText.text = $"• {recipe.OutputAmount}x {recipe.Output.Name}";
        _recipeDurationText.text = $"{recipe.Duration}s";
    }
}