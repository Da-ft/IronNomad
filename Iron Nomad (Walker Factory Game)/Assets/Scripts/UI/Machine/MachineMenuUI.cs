using IronNomad.Inputs;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MachineMenuUI : MonoBehaviour, IMenu
{
    public static MachineMenuUI Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;

    [Header("UI References")]
    [SerializeField] private GameObject _menuRoot;
    [SerializeField] private TextMeshProUGUI _machineName;
    [SerializeField] private Transform _recipeList;

    [Header("Recipe Info")]
    [SerializeField] private GameObject _recipeInfoPanel;
    [SerializeField] private TextMeshProUGUI _recipeInputText;
    [SerializeField] private TextMeshProUGUI _recipeOutputText;
    [SerializeField] private TextMeshProUGUI _recipeDurationText;

    [Header("Prefabs")]
    [SerializeField] private GameObject _recipeButtonPrefab;

    public bool IsOpen => _isOpen;
    private bool _isOpen = false;

    private IMachine _currentMachine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        UIManager.Instance.RegisterMenu(this);
        _menuRoot.SetActive(false);
        _recipeInfoPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        UIManager.Instance?.UnregisterMenu(this);
    }

    public void Open(IMachine machine)
    {
        _currentMachine = machine;
        UIManager.Instance.OpenMenu(this);
    }

    // --- IMenu ---

    public void Open()
    {
        _isOpen = true;
        _menuRoot.SetActive(true);

        // Einen Frame warten bevor Gameplay disabled wird
        StartCoroutine(DisableNextFrame());

        BuildRecipeList();
        if (_currentMachine.GetCurrentRecipe() != null)
            ShowRecipeInfo(_currentMachine.GetCurrentRecipe());
    }

    public void Close()
    {
        _isOpen = false;
        _menuRoot.SetActive(false);
        _recipeInfoPanel.SetActive(false);
        _currentMachine = null;
    }

    // --- Recipe List ---

    private void BuildRecipeList()
    {
        foreach (Transform child in _recipeList)
            Destroy(child.gameObject);

        CraftingRecipe[] recipes = _currentMachine.GetRecipes();

        _machineName.text = recipes.Length > 0 ? "Rezepte" : "Keine Rezepte verfügbar";

        foreach (CraftingRecipe recipe in recipes)
        {
            GameObject obj = Instantiate(_recipeButtonPrefab, _recipeList);

            // Button Text
            TextMeshProUGUI label = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = recipe.Output.Name;

            // Highlight falls aktuelles Rezept
            Image bg = obj.GetComponent<Image>();
            if (bg != null)
                bg.color = recipe == _currentMachine.GetCurrentRecipe()
                    ? new Color(0.3f, 0.8f, 0.3f, 1f)
                    : Color.white;

            Button btn = obj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => Debug.Log("CLICK"));  // ← temporär
                CraftingRecipe captured = recipe;
                btn.onClick.AddListener(() => OnRecipeSelected(captured));
            }
        }
    }

    private void OnRecipeSelected(CraftingRecipe recipe)
    {
        _currentMachine.SetRecipe(recipe);
        ShowRecipeInfo(recipe);
        BuildRecipeList(); // Highlight updaten
    }

    private void ShowRecipeInfo(CraftingRecipe recipe)
    {
        _recipeInfoPanel.SetActive(true);

        // Inputs zusammenbauen
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