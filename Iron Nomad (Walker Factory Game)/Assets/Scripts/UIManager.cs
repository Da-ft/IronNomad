using IronNomad.Inputs;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private List<IMenu> _menus = new List<IMenu>();
    [SerializeField] private InputReader _inputReader;

    private void OnEnable() => _inputReader.CloseMenuEvent += CloseAll;
    private void OnDisable() => _inputReader.CloseMenuEvent -= CloseAll;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
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

    public void OpenMenu(IMenu menu)
    {
        // Alle anderen schlieﬂen
        foreach (var m in _menus)
        {
            if (m != menu && m.IsOpen)
                m.Close();
        }
        menu.Open();
    }

    public void CloseAll()
    {
        foreach (var menu in _menus)
        {
            if (menu.IsOpen) menu.Close();
        }
    }
}