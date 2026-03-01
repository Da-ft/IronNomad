public class NoneState : ToolState
{
    public override void Enter() { }
    public override void Exit() { }
}

// --- BuilderState ---
public class BuilderState : ToolState
{
    private readonly BuilderTool _tool;
    private readonly BuildMenu _menu;

    public BuilderState(BuilderTool tool, BuildMenu menu)
    {
        _tool = tool;
        _menu = menu;
    }

    public override void Enter()
    {
        _tool.gameObject.SetActive(true);
        _tool.OnEquip();
        _menu.SetEnabled(true, _tool);
        UIManager.Instance.OpenMenu(_menu);
    }

    public override void Exit()
    {
        _tool.OnUnequip();
        _tool.gameObject.SetActive(false);
        _menu.SetEnabled(false, _tool);
    }
}

// --- DemolishState ---
public class DemolishState : ToolState
{
    private readonly DemolishTool _tool;

    public DemolishState(DemolishTool tool)
    {
        _tool = tool;
    }

    public override void Enter()
    {
        _tool.gameObject.SetActive(true);
        _tool.OnEquip();
    }

    public override void Exit()
    {
        _tool.OnUnequip();
        _tool.gameObject.SetActive(false);
    }
}

// --- MinerState ---
public class MinerState : ToolState
{
    private readonly MiningTool _tool;

    public MinerState(MiningTool tool)
    {
        _tool = tool;
    }

    public override void Enter()
    {
        _tool.gameObject.SetActive(true);
        _tool.OnEquip();
    }

    public override void Exit()
    {
        _tool.OnUnequip();
        _tool.gameObject.SetActive(false);
    }
}