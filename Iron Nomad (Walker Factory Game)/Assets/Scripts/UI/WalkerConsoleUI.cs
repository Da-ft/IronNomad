using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Die Walker-Konsole — einziges IMenu für alle Walker-Sub-Panels.
/// Reiter schalten zwischen TechTree, Strom, etc. um.
/// </summary>
public class WalkerConsoleUI : MonoBehaviour, IMenu
{
    [Header("Root")]
    [SerializeField] private GameObject _menuRoot;

    [Header("Tabs")]
    [SerializeField] private Transform _tabBar;
    [SerializeField] private GameObject _tabButtonPrefab;
    [SerializeField] private List<ConsoleTab> _tabs;

    public bool IsOpen => _isOpen;
    private bool _isOpen = false;
    private int _activeTab = -1;

    private void Start()
    {
        UIManager.Instance.RegisterMenu(this);
        _menuRoot.SetActive(false);
        BuildTabBar();
    }

    private void OnDestroy() => UIManager.Instance?.UnregisterMenu(this);

    // --- IMenu ---

    public void Open()
    {
        _isOpen = true;
        _menuRoot.SetActive(true);
        ShowTab(_activeTab >= 0 ? _activeTab : 0);
    }

    public void Close()
    {
        _isOpen = false;
        _menuRoot.SetActive(false);
        NotifyTabClosed(_activeTab);
    }

    // --- Tabs ---

    private void BuildTabBar()
    {
        foreach (Transform child in _tabBar) Destroy(child.gameObject);

        for (int i = 0; i < _tabs.Count; i++)
        {
            int index = i;
            GameObject obj = Instantiate(_tabButtonPrefab, _tabBar);
            Button btn = obj.GetComponent<Button>();
            TextMeshProUGUI label = obj.GetComponentInChildren<TextMeshProUGUI>();

            if (label != null) label.text = _tabs[i].TabName;
            btn.onClick.AddListener(() => ShowTab(index));

            // Panel initial ausblenden
            _tabs[i].Panel.SetActive(false);
        }
    }

    private void ShowTab(int index)
    {
        if (index < 0 || index >= _tabs.Count) return;

        NotifyTabClosed(_activeTab);

        _activeTab = index;

        for (int i = 0; i < _tabs.Count; i++)
            _tabs[i].Panel.SetActive(i == index);

        NotifyTabOpened(_activeTab);
    }

    private void NotifyTabOpened(int index)
    {
        if (index < 0 || index >= _tabs.Count) return;
        _tabs[index].Panel.GetComponent<TechTreeMenuUI>()?.OnTabOpened();
        _tabs[index].Panel.GetComponent<WalkerPowerMenuUI>()?.OnTabOpened();
    }

    private void NotifyTabClosed(int index)
    {
        if (index < 0 || index >= _tabs.Count) return;
        _tabs[index].Panel.GetComponent<TechTreeMenuUI>()?.OnTabClosed();
        _tabs[index].Panel.GetComponent<WalkerPowerMenuUI>()?.OnTabClosed();
    }
}

[System.Serializable]
public class ConsoleTab
{
    public string TabName;
    public GameObject Panel;
}