using UnityEngine;

public class ScrapPile : MonoBehaviour, IMineable
{
    [Header("Config")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private ItemDefinition _dropItem;
    [SerializeField] private int _dropAmount = 3;

    [Header("Dependencies")]
    [SerializeField] private InventorySystem _inventory;

    private float _currentHealth;

    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;

    private void Start()
    {
        _currentHealth = _maxHealth;

        // Inventory vom Player holen falls nicht gesetzt
        if (_inventory == null)
            _inventory = FindAnyObjectByType<InventorySystem>();
    }

    public void Mine(float damage)
    {
        _currentHealth -= damage;
        Debug.Log($"{gameObject.name}: {_currentHealth}/{_maxHealth} HP");

        if (_currentHealth <= 0)
            Deplete();
    }

    private void Deplete()
    {
        if (_dropItem != null && _inventory != null)
        {
            _inventory.AddItem(_dropItem, _dropAmount);
            Debug.Log($"+{_dropAmount}x {_dropItem.Name} ins Inventar!");
        }

        Destroy(gameObject);
    }
}








