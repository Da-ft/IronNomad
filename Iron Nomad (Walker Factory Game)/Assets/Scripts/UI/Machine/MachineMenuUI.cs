using IronNomad.Inputs;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MachineMenuUI : MonoBehaviour, IMenu
{
    public static MachineMenuUI Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;

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
    [SerializeField] private GameObject _recipeButtonPrefab;

    public bool IsOpen => _isOpen;
    private bool _isOpen = false;
    private IMachine _currentMachine;

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

    private void OnDestroy()
    {
        UIManager.Instance?.UnregisterMenu(this);
    }

    private void Update()
    {
        if (!_isOpen || _currentMachine == null) return;
        if (_viewProduction == null || !_viewProduction.activeSelf) return;

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
        StartCoroutine(DisableNextFrame());

        // Kein Rezept aktiv → Rezeptauswahl zeigen
        if (_currentMachine.GetCurrentRecipe() == null)
            ShowRecipeSelection();
        else
            ShowProduction();
    }

    public void Close()
    {
        _isOpen = false;
        _menuRoot.SetActive(false);
        _currentMachine = null;
    }

    // --- Views ---

    private void ShowRecipeSelection()
    {
        _viewRecipeSelection.SetActive(true);
        _viewProduction.SetActive(false);
        _recipeInfoPanel.SetActive(false);
        _btnSelectRecipe.interactable = false;

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
            _progressFill.sprite = active.Output.Icon;  // ← NEU
            _progressFill.color = Color.white;
        }
    }

    // --- Recipe Selection ---

    private void BuildRecipeList()
    {
        foreach (Transform child in _recipeList)
            DestroyImmediate(child.gameObject);

        CraftingRecipe[] recipes = _currentMachine.GetRecipes();
        _machineName.text = recipes.Length > 0 ? "Rezept wählen" : "Keine Rezepte verfügbar";

        foreach (CraftingRecipe recipe in recipes)
        {
            GameObject obj = Instantiate(_recipeButtonPrefab, _recipeList);

            TextMeshProUGUI label = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = recipe.Output.Name;

            Image bg = obj.GetComponent<Image>();

            Button btn = obj.GetComponent<Button>();
            if (btn != null)
            {
                CraftingRecipe captured = recipe;
                btn.onClick.AddListener(() => OnRecipeHighlighted(captured, bg));
            }
        }
    }

    private CraftingRecipe _highlightedRecipe;

    private void OnRecipeHighlighted(CraftingRecipe recipe, Image bg)
    {
        // Alle Buttons zurücksetzen
        foreach (Transform child in _recipeList)
        {
            Image childBg = child.GetComponent<Image>();
            if (childBg != null) childBg.color = Color.white;
        }

        // Diesen highlighten
        if (bg != null) bg.color = new Color(0.3f, 0.8f, 0.3f, 1f);

        _highlightedRecipe = recipe;
        _btnSelectRecipe.interactable = true;

        // Info anzeigen
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

        System.Text.StringBuilder inputs = new System.Text.StringBuilder();
        for (int i = 0; i < recipe.Inputs.Length; i++)
        {
            int amount = i < recipe.InputAmounts.Length ? recipe.InputAmounts[i] : 1;
            inputs.AppendLine($"• {amount}x {recipe.Inputs[i].Name}");
        }
        _recipeInputText.text = inputs.ToString();
        _recipeOutputText.text = $"• {recipe.OutputAmount}x {recipe.Output.Name}";
        _recipeDurationText.text = $"{recipe.Duration}s";
    }

    private System.Collections.IEnumerator DisableNextFrame()
    {
        yield return null;
        _inputReader.DisableGameplay();
    }
}