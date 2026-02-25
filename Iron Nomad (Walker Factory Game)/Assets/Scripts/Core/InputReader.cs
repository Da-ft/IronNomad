using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace IronNomad.Inputs
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "IronNomad/InputReader")]
    public class InputReader : ScriptableObject
    {
        // --- Move Vars ---
        public event UnityAction<Vector2> MoveEvent;
        public event UnityAction<Vector2> LookEvent;
        public event UnityAction JumpEvent;
        public event UnityAction<bool> SprintEvent;

        // --- Inventory System (Inventar und Interaktion) ---
        public event UnityAction InteractEvent;
        public event UnityAction<float> ScrollEvent;
        public event UnityAction<int> HotbarSelectEvent;

        // --- Build Mode ---
        public event UnityAction BuildModeEvent;
        public event UnityAction RotateEvent;
        public event UnityAction PlaceEvent;

        // --- Interface Implementation ---
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
            if (context.performed) JumpEvent?.Invoke();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.performed) SprintEvent?.Invoke(true);
            else if (context.canceled) SprintEvent?.Invoke(false);
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed) InteractEvent?.Invoke();
        }

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
        public void OnToggleBuild(InputAction.CallbackContext context)
        {
            if (context.performed) BuildModeEvent?.Invoke();
        }

        public void OnRotate(InputAction.CallbackContext context)
        {
            if (context.performed) RotateEvent?.Invoke();
        }

        public void OnPlace(InputAction.CallbackContext context)
        {
            if (context.performed) PlaceEvent?.Invoke();
        }
    }
}