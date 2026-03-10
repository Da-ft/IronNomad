using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private List<IMenu> _menus = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnEnable() => InputEvents.OnCloseMenu += CloseAll;
    private void OnDisable() => InputEvents.OnCloseMenu -= CloseAll;

    public void RegisterMenu(IMenu menu) { if (!_menus.Contains(menu)) _menus.Add(menu); }
    public void UnregisterMenu(IMenu menu) => _menus.Remove(menu);

    public void OpenMenu(IMenu menu)
    {
        foreach (var m in _menus)
            if (m != menu && m.IsOpen) m.Close();

        menu.Open();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        InputReader.Instance.DisableGameplay();
        InputReader.Instance.ResetLook();
        InputReader.Instance.ResetMove();
    }

    public void OpenMenuAdditive(IMenu menu)
    {
        if (!menu.IsOpen) menu.Open();
    }

    public void CloseAll()
    {
        foreach (var m in _menus)
            if (m.IsOpen) m.Close();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        InputReader.Instance.EnableGameplay();
    }

    public bool AnyMenuOpen()
    {
        foreach (var m in _menus)
            if (m.IsOpen) return true;
        return false;
    }
}