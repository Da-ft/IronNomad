using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace IronNomad.Inputs
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "IronNomad/InputReader")]
    public class InputReader : ScriptableObject
    {
        private bool _inputEnabled = true;

        private void OnEnable() => _inputEnabled = true;

        // --- Events ---
        public event UnityAction<Vector2> MoveEvent;
        public event UnityAction<Vector2> LookEvent;
        public event UnityAction JumpEvent;
        public event UnityAction<bool> SprintEvent;

        public event UnityAction InteractEvent;
        public event UnityAction<float> ScrollEvent;
        public event UnityAction<int> HotbarSelectEvent;

        public event UnityAction BuildModeEvent;
        public event UnityAction RotateEvent;
        public event UnityAction PlaceEvent;

        public event UnityAction DemolishEvent;
        public event UnityAction DemolishConfirmEvent;

        public event UnityAction InventoryEvent;
        public event UnityAction CloseMenuEvent;

        // --- Input Callbacks ---

        public void OnMove(InputAction.CallbackContext context)
        {
            if (!_inputEnabled) return;
            MoveEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (!_inputEnabled) return;
            LookEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (!_inputEnabled) return;
            if (context.performed) JumpEvent?.Invoke();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (!_inputEnabled) return;
            if (context.performed) SprintEvent?.Invoke(true);
            else if (context.canceled) SprintEvent?.Invoke(false);
        }

        // Interact feuert immer – wenn Gameplay disabled, dient es als "Menü schließen"
        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (_inputEnabled)
                InteractEvent?.Invoke();
            else
                CloseMenuEvent?.Invoke();
        }

        // Kein _inputEnabled check - Scroll feuert immer (z.B. Zoom im Techtree)
        public void OnScroll(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                float scroll = context.ReadValue<Vector2>().y;
                if (scroll > 0) ScrollEvent?.Invoke(1f);
                else if (scroll < 0) ScrollEvent?.Invoke(-1f);
            }
        }

        public void OnHotbar1(InputAction.CallbackContext context) { if (context.performed) HotbarSelectEvent?.Invoke(0); }
        public void OnHotbar2(InputAction.CallbackContext context) { if (context.performed) HotbarSelectEvent?.Invoke(1); }
        public void OnHotbar3(InputAction.CallbackContext context) { if (context.performed) HotbarSelectEvent?.Invoke(2); }
        public void OnHotbar4(InputAction.CallbackContext context) { if (context.performed) HotbarSelectEvent?.Invoke(3); }
        public void OnHotbar5(InputAction.CallbackContext context) { if (context.performed) HotbarSelectEvent?.Invoke(4); }
        public void OnHotbar6(InputAction.CallbackContext context) { if (context.performed) HotbarSelectEvent?.Invoke(5); }
        public void OnHotbar7(InputAction.CallbackContext context) { if (context.performed) HotbarSelectEvent?.Invoke(6); }
        public void OnHotbar8(InputAction.CallbackContext context) { if (context.performed) HotbarSelectEvent?.Invoke(7); }
        public void OnHotbar9(InputAction.CallbackContext context) { if (context.performed) HotbarSelectEvent?.Invoke(8); }

        public void OnRotate(InputAction.CallbackContext context)
        {
            if (!_inputEnabled) return;
            if (context.performed) RotateEvent?.Invoke();
        }

        public void OnPlace(InputAction.CallbackContext context)
        {
            if (!_inputEnabled) return;
            if (context.performed) PlaceEvent?.Invoke();
        }

        public void OnDemolish(InputAction.CallbackContext context)
        {
            if (!_inputEnabled) return;
            if (context.performed) DemolishEvent?.Invoke();
        }

        public void OnDemolishConfirm(InputAction.CallbackContext context)
        {
            if (!_inputEnabled) return;
            if (context.performed) DemolishConfirmEvent?.Invoke();
        }

        // Kein _inputEnabled check - Menü-Tasten feuern immer!
        public void OnToggleBuild(InputAction.CallbackContext context)
        {
            if (context.performed) BuildModeEvent?.Invoke();
        }

        public void OnInventory(InputAction.CallbackContext context)
        {
            if (context.performed) InventoryEvent?.Invoke();
        }

        public void OnCloseMenu(InputAction.CallbackContext context)
        {
            if (context.performed) CloseMenuEvent?.Invoke();
        }

        // --- Helpers ---

        public void DisableGameplay() => _inputEnabled = false;
        public void EnableGameplay() => _inputEnabled = true;

        public void ResetLook() => LookEvent?.Invoke(Vector2.zero);
        public void ResetMove() => MoveEvent?.Invoke(Vector2.zero);
    }
}