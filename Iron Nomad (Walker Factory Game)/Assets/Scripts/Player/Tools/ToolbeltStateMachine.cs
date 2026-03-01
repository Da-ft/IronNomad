using UnityEngine;
using IronNomad.Inputs;

public class ToolbeltStateMachine : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;

    [Header("Tool References")]
    [SerializeField] private BuilderTool _builderTool;
    [SerializeField] private DemolishTool _demolishTool;
    [SerializeField] private MiningTool _miningTool;

    [Header("Menus")]
    [SerializeField] private BuildMenu _buildMenu;

    private NoneState _noneState;
    private BuilderState _builderState;
    private DemolishState _demolishState;
    private MinerState _minerState;

    private ToolState _currentState;
    private int _activeHotkey = -1;

    public ToolState CurrentState => _currentState;

    private void Awake()
    {
        _builderTool.gameObject.SetActive(false);
        _demolishTool.gameObject.SetActive(false);
        _miningTool.gameObject.SetActive(false);

        _noneState = new NoneState();
        _builderState = new BuilderState(_builderTool, _buildMenu);
        _demolishState = new DemolishState(_demolishTool);
        _minerState = new MinerState(_miningTool);
    }

    private void Start()
    {
        TransitionTo(_noneState, -1);
    }

    private void OnEnable()
    {
        _inputReader.HotbarSelectEvent += HandleHotkey;
        _inputReader.CloseMenuEvent += ReturnToDefault;
    }

    private void OnDisable()
    {
        _inputReader.HotbarSelectEvent -= HandleHotkey;
        _inputReader.CloseMenuEvent -= ReturnToDefault;
    }

    private void HandleHotkey(int index)
    {
        // Nochmal gleicher Hotkey → zurück zu None
        if (index == _activeHotkey)
        {
            TransitionTo(_noneState, -1);
            return;
        }

        switch (index)
        {
            case 0: TransitionTo(_builderState, 0); break;
            case 1: TransitionTo(_demolishState, 1); break;
            case 2: TransitionTo(_minerState, 2); break;
        }
    }

    private void ReturnToDefault()
    {
        if (_activeHotkey == -1) return; // Schon in None, nichts tun
        TransitionTo(_noneState, -1);
    }

    private void TransitionTo(ToolState newState, int hotkey)
    {
        _currentState?.Exit();
        _activeHotkey = hotkey;
        _currentState = newState;
        _currentState.Enter();
    }
}