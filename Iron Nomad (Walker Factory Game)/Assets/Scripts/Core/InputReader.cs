using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using IronNomad.Inputs;

[CreateAssetMenu(fileName = "InputReader", menuName = "IronNomad/InputReader")]
public class InputReader : ScriptableObject, GameInput.IGameplayActions
{
    public event UnityAction<Vector2> MoveEvent;
    public event UnityAction<Vector2> LookEvent;
    public event UnityAction JumpEvent;
    public event UnityAction JumpCanceledEvent;
    public event UnityAction BuildEvent;

    private GameInput _gameInput;

    private void OnEnable()
    {
        if (_gameInput == null)
        {
            _gameInput = new GameInput();
            _gameInput.Gameplay.SetCallbacks(this);
        }
        _gameInput.Gameplay.Enable();
    }

    private void OnDisable()
    {
        _gameInput?.Gameplay.Disable();
    }

    // Interface Implementation
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            JumpEvent?.Invoke();
        if(context.phase == InputActionPhase.Canceled)
            JumpCanceledEvent?.Invoke();
    }

    public void OnBuild(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
            BuildEvent?.Invoke();
    }
}
