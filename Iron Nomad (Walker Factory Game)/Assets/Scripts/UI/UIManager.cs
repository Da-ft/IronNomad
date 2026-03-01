using IronNomad.Inputs;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private InputReader _inputReader;
    private List<IMenu> _menus = new List<IMenu>();

    private void OnEnable() => _inputReader.CloseMenuEvent += CloseAll;
    private void OnDisable() => _inputReader.CloseMenuEvent -= CloseAll;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RegisterMenu(IMenu menu)
    {
        if (!_menus.Contains(menu))
            _menus.Add(menu);
    }

    public void UnregisterMenu(IMenu menu)
    {
        _menus.Remove(menu);
    }

    // Schließt alle anderen Menüs, dann öffnet dieses
    public void OpenMenu(IMenu menu)
    {
        foreach (var m in _menus)
            if (m != menu && m.IsOpen) m.Close();
        menu.Open();
    }

    // Öffnet ein Menü zusätzlich ohne andere zu schließen
    public void OpenMenuAdditive(IMenu menu)
    {
        if (!menu.IsOpen) menu.Open();
    }

    public bool AnyMenuOpen()
    {
        foreach (var m in _menus)
            if (m.IsOpen) return true;
        return false;
    }

    // Schließt ALLE Menüs und gibt danach Gameplay + Cursor frei
    public void CloseAll()
    {
        foreach (var m in _menus)
            if (m.IsOpen) m.Close();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _inputReader.EnableGameplay();
    }
}