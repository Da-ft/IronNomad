using UnityEngine;

public class PlayerToolController : MonoBehaviour
{
    public enum ToolState { None, BuildMenu, Placing, Demolish }

    [Header("UI")]
    [SerializeField] private BuildMenu _buildMenu;

    [Header("Tools")]
    [SerializeField] private BuilderTool _builderTool;
    [SerializeField] private DemolishTool _demolishTool;

    public ToolState Current { get; private set; } = ToolState.None;

    private void Awake()
    {
        _buildMenu.SetBuilderTool(_builderTool);
        _buildMenu.OnBuildingSelected += OnBuildingSelectedFromMenu;
    }

    private void OnEnable()
    {
        InputEvents.OnBuildMode += ToggleBuildMenu;
        InputEvents.OnDemolish += ToggleDemolish;
        InputEvents.OnCloseMenu += Cancel;
    }

    private void OnDisable()
    {
        InputEvents.OnBuildMode -= ToggleBuildMenu;
        InputEvents.OnDemolish -= ToggleDemolish;
        InputEvents.OnCloseMenu -= Cancel;
    }

    private void ToggleBuildMenu()
    {
        if (Current == ToolState.BuildMenu || Current == ToolState.Placing) Cancel();
        else EnterBuildMenu();
    }

    private void ToggleDemolish()
    {
        if (Current == ToolState.Demolish) Cancel();
        else EnterDemolish();
    }

    private void EnterBuildMenu()
    {
        ExitCurrent();
        Current = ToolState.BuildMenu;
        _builderTool.OnEquip();
        UIManager.Instance.OpenMenu(_buildMenu);
    }

    private void OnBuildingSelectedFromMenu(BuildingDefinition def)
    {
        UIManager.Instance.CloseAll();
        Current = ToolState.Placing;
        _builderTool.SelectBuilding(def);
    }

    private void EnterDemolish()
    {
        ExitCurrent();
        Current = ToolState.Demolish;
        _demolishTool.OnEquip();
    }

    private void Cancel()
    {
        ExitCurrent();
        Current = ToolState.None;
    }

    private void ExitCurrent()
    {
        switch (Current)
        {
            case ToolState.BuildMenu:
            case ToolState.Placing:
                _builderTool.OnUnequip();
                if (UIManager.Instance.AnyMenuOpen()) UIManager.Instance.CloseAll();
                break;
            case ToolState.Demolish:
                _demolishTool.OnUnequip();
                break;
        }
    }
}