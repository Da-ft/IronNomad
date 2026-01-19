using UnityEngine;
using System.Collections.Generic;

public class WalkerMiningCore : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private ItemDefinition _resourceToMine;
    [SerializeField] private float _itemsPerMinute = 10;

    [Header("Connections")]
    [SerializeField] private List<OutputShaft> _outputShafts = new List<OutputShaft>();

    private float _timer;

    private void Update()
    {
        // Simple Timer
        float timePerItem = 60f / _itemsPerMinute;
        _timer += Time.deltaTime;

        if (_timer >= timePerItem)
        {
            Debug.Log("We Try to distribute!");
            TryDistributeOre();
            {
                _timer = 0f; // Reset or -= timePerItem für exaktere Zeit
            }
        }
    }

    private void TryDistributeOre()
    {
        // Ersten freien Schacht suchen
        foreach (var shaft in _outputShafts)
        {
            // Try deposit Item
            if (shaft.TryDeposit(_resourceToMine))
            {
                // We did it!
                Debug.Log("Erz gefördert!");
                return;
            }
        }
        // TODO: alle Schächte voll -> Produktion stop!
        Debug.Log("Alle Schächte voll!");
    }
}
