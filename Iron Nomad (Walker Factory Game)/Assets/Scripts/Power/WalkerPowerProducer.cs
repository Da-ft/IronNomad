using UnityEngine;

/// <summary>
/// Auf dem Walker-GameObject. Registriert den Walker als Stromquelle.
/// </summary>
public class WalkerPowerProducer : MonoBehaviour, IPowerProducer
{
    [SerializeField] private float _powerOutput = 50f; // MW
    public float PowerOutput => _powerOutput;

    private void OnEnable() => PowerGrid.Instance?.RegisterProducer(this);
    private void OnDisable() => PowerGrid.Instance?.UnregisterProducer(this);
}