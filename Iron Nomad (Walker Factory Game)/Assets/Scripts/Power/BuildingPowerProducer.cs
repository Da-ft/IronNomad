using UnityEngine;

/// <summary>
/// Generische Producer-Komponente — auf jedes Gebäude-Prefab legen das Strom produziert.
/// PowerProduction kommt aus der BuildingDefinition.
/// Funktioniert für Solarpanels, Generatoren, etc. ohne extra Code.
/// </summary>
[RequireComponent(typeof(ConstructibleBuilding))]
public class BuildingPowerProducer : MonoBehaviour, IPowerProducer
{
    private ConstructibleBuilding _constructible;

    public float PowerOutput => _constructible?.Definition?.PowerProduction ?? 0f;

    private void Awake()
    {
        _constructible = GetComponent<ConstructibleBuilding>();
    }

    private void OnEnable()
    {
        if (_constructible?.Definition?.IsProducer ?? false)
            PowerGrid.Instance?.RegisterProducer(this);
    }

    private void OnDisable()
    {
        PowerGrid.Instance?.UnregisterProducer(this);
    }
}