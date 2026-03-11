using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Verbindet Consumer im Radius mit dem PowerGrid.
/// Alle PowerPoles rescannen wenn ein neues Gebäude gebaut wird.
/// </summary>
public class PowerPole : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float _radius = 10f;
    [SerializeField] private LayerMask _buildingLayer;

    // Alle aktiven PowerPoles — damit neu platzierte Gebäude sofort erfasst werden
    private static readonly List<PowerPole> _allPoles = new();

    private readonly List<IPowerConsumer> _coveredConsumers = new();

    private void Start()
    {
        _allPoles.Add(this);
        ScanAndConnect();
    }

    private void OnDestroy()
    {
        _allPoles.Remove(this);
        Disconnect();
    }

    /// <summary>
    /// Wird von BaseGridMachine.Start() aufgerufen damit alle Poles neu scannen.
    /// </summary>
    public static void NotifyNewBuilding()
    {
        foreach (var pole in _allPoles)
            pole.ScanAndConnect();
    }

    private void ScanAndConnect()
    {
        // Alte Verbindungen lösen
        Disconnect();

        Collider[] hits = Physics.OverlapSphere(transform.position, _radius, _buildingLayer);
        foreach (var hit in hits)
        {
            IPowerConsumer consumer = hit.GetComponentInParent<IPowerConsumer>();
            if (consumer == null) continue;
            if (_coveredConsumers.Contains(consumer)) continue;

            _coveredConsumers.Add(consumer);
            PowerGrid.Instance?.RegisterConsumer(consumer);
        }
    }

    private void Disconnect()
    {
        foreach (var c in _coveredConsumers)
            PowerGrid.Instance?.UnregisterConsumer(c);
        _coveredConsumers.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, _radius);
        Gizmos.color = new Color(1f, 1f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}