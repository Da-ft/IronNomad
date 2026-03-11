using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Globaler Strom-Pool. Berechnet jede Sekunde ob genug Strom da ist.
/// Producer und Consumer registrieren sich selbst.
/// </summary>
public class PowerGrid : MonoBehaviour
{
    public static PowerGrid Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private float _updateInterval = 1f;

    private readonly HashSet<IPowerProducer> _producers = new();
    private readonly HashSet<IPowerConsumer> _consumers = new();

    private float _timer;

    // --- Öffentliche Werte für die UI ---
    public float TotalProduction { get; private set; }
    public float TotalDemand { get; private set; }
    public bool HasEnoughPower => TotalProduction >= TotalDemand;

    public event System.Action OnPowerStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < _updateInterval) return;
        _timer = 0f;
        Recalculate();
    }

    // --- Registrierung ---

    public void RegisterProducer(IPowerProducer p) => _producers.Add(p);
    public void UnregisterProducer(IPowerProducer p) => _producers.Remove(p);
    public void RegisterConsumer(IPowerConsumer c) => _consumers.Add(c);
    public void UnregisterConsumer(IPowerConsumer c) => _consumers.Remove(c);

    // --- Berechnung ---

    private void Recalculate()
    {
        float production = 0f;
        foreach (var p in _producers) production += p.PowerOutput;

        float demand = 0f;
        foreach (var c in _consumers) demand += c.PowerDemand;

        TotalProduction = production;
        TotalDemand = demand;

        bool powered = production >= demand;

        // Alle Consumer informieren
        foreach (var c in _consumers)
            c.IsPowered = powered;

        OnPowerStateChanged?.Invoke();
    }
}