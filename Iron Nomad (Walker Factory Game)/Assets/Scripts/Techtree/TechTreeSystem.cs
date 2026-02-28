using System.Collections.Generic;
using UnityEngine;

public class TechTreeSystem : MonoBehaviour
{
    public static TechTreeSystem Instance { get; private set; }

    private HashSet<ItemDefinition> _discoveredItems = new HashSet<ItemDefinition>();

    public event System.Action<ItemDefinition> OnItemDiscovered;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool IsDiscovered(ItemDefinition item) => _discoveredItems.Contains(item);

    public bool Discover(ItemDefinition item)
    {
        if (item == null || _discoveredItems.Contains(item)) return false;

        _discoveredItems.Add(item);
        OnItemDiscovered?.Invoke(item);
        Debug.Log($"[TechTree] '{item.Name}' entdeckt!");
        return true;
    }
}