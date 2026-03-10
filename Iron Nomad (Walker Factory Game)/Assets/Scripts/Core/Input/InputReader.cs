using UnityEngine;
using UnityEngine.InputSystem;
using IronNomad.Inputs;

/// <summary>
/// Singleton MonoBehaviour. Liest GameInput und feuert InputEvents.
/// Alle anderen Klassen abonnieren InputEvents direkt — keine Referenz auf InputReader nötig.
/// </summary>
public class InputReader : MonoBehaviour
{
    public static InputReader Instance { get; private set; }

    private GameInput _input;
    private bool _inputEnabled = true;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        _input = new GameInput();
    }

    private void OnEnable()
    {
        _input.Gameplay.Enable();

        _input.Gameplay.Move.performed += ctx => { if (_inputEnabled) InputEvents.FireMove(ctx.ReadValue<Vector2>()); };
        _input.Gameplay.Move.canceled += ctx => { if (_inputEnabled) InputEvents.FireMove(Vector2.zero); };
        _input.Gameplay.Look.performed += ctx => { if (_inputEnabled) InputEvents.FireLook(ctx.ReadValue<Vector2>()); };
        _input.Gameplay.Look.canceled += ctx => InputEvents.FireLook(Vector2.zero);
        _input.Gameplay.Jump.performed += ctx => { if (_inputEnabled) InputEvents.FireJump(); };
        _input.Gameplay.Sprint.performed += ctx => { if (_inputEnabled) InputEvents.FireSprint(true); };
        _input.Gameplay.Sprint.canceled += ctx => { if (_inputEnabled) InputEvents.FireSprint(false); };
        _input.Gameplay.Interact.performed += ctx => { if (_inputEnabled) InputEvents.FireInteract(); };
        _input.Gameplay.Scroll.performed += ctx =>
        {
            float y = ctx.ReadValue<Vector2>().y;
            if (y != 0) InputEvents.FireScroll(y > 0 ? 1f : -1f);
        };

        _input.Gameplay.Place.performed += ctx => { if (_inputEnabled) InputEvents.FirePlace(); };
        _input.Gameplay.Rotate.performed += ctx => { if (_inputEnabled) InputEvents.FireRotate(); };
        _input.Gameplay.Demolish.performed += ctx => { if (_inputEnabled) InputEvents.FireDemolish(); };
        _input.Gameplay.DemolishConfirm.performed += ctx => { if (_inputEnabled) InputEvents.FireDemolishConfirm(); };

        // Diese feuern immer (auch wenn Gameplay disabled)
        _input.Gameplay.ToggleBuild.performed += ctx => {
            Debug.Log("[InputReader] ToggleBuild gefeuert");
            InputEvents.FireBuildMode();
        }; _input.Gameplay.Inventory.performed += ctx => InputEvents.FireInventory();
        _input.Gameplay.CloseMenu.performed += ctx => InputEvents.FireCloseMenu();

        _input.Gameplay.Hotbar1.performed += _ => InputEvents.FireHotbarSelect(0);
        _input.Gameplay.Hotbar2.performed += _ => InputEvents.FireHotbarSelect(1);
        _input.Gameplay.Hotbar3.performed += _ => InputEvents.FireHotbarSelect(2);
        _input.Gameplay.Hotbar4.performed += _ => InputEvents.FireHotbarSelect(3);
        _input.Gameplay.Hotbar5.performed += _ => InputEvents.FireHotbarSelect(4);
        _input.Gameplay.Hotbar6.performed += _ => InputEvents.FireHotbarSelect(5);
        _input.Gameplay.Hotbar7.performed += _ => InputEvents.FireHotbarSelect(6);
        _input.Gameplay.Hotbar8.performed += _ => InputEvents.FireHotbarSelect(7);
        _input.Gameplay.Hotbar9.performed += _ => InputEvents.FireHotbarSelect(8);
    }

    private void OnDisable() => _input.Gameplay.Disable();

    public void EnableGameplay() => _inputEnabled = true;
    public void DisableGameplay() => _inputEnabled = false;

    // Kompatibilität mit UIManager
    public void ResetLook() => InputEvents.FireLook(Vector2.zero);
    public void ResetMove() => InputEvents.FireMove(Vector2.zero);
}