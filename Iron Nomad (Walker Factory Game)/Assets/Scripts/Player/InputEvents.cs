using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Statischer Event-Bus für alle Input-Events.
/// Keine Referenzen nötig — einfach direkt abonnieren:
///   InputEvents.OnPlace += MyMethod;
/// </summary>
public static class InputEvents
{
    public static event UnityAction<Vector2> OnMove;
    public static event UnityAction<Vector2> OnLook;
    public static event UnityAction OnJump;
    public static event UnityAction<bool> OnSprint;
    public static event UnityAction OnInteract;
    public static event UnityAction<float> OnScroll;
    public static event UnityAction OnPlace;
    public static event UnityAction OnRotate;
    public static event UnityAction OnBuildMode;
    public static event UnityAction OnDemolish;
    public static event UnityAction OnDemolishConfirm;
    public static event UnityAction OnInventory;
    public static event UnityAction OnCloseMenu;
    public static event UnityAction<int> OnHotbarSelect;

    // Fire-Methoden — nur InputReader ruft diese auf
    public static void FireMove(Vector2 v) => OnMove?.Invoke(v);
    public static void FireLook(Vector2 v) => OnLook?.Invoke(v);
    public static void FireJump() => OnJump?.Invoke();
    public static void FireSprint(bool v) => OnSprint?.Invoke(v);
    public static void FireInteract() => OnInteract?.Invoke();
    public static void FireScroll(float v) => OnScroll?.Invoke(v);
    public static void FirePlace() => OnPlace?.Invoke();
    public static void FireRotate() => OnRotate?.Invoke();
    public static void FireBuildMode() => OnBuildMode?.Invoke();
    public static void FireDemolish() => OnDemolish?.Invoke();
    public static void FireDemolishConfirm() => OnDemolishConfirm?.Invoke();
    public static void FireInventory() => OnInventory?.Invoke();
    public static void FireCloseMenu() => OnCloseMenu?.Invoke();
    public static void FireHotbarSelect(int i) => OnHotbarSelect?.Invoke(i);
}