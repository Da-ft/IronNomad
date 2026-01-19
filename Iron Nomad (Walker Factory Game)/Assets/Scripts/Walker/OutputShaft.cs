using UnityEngine;

public class OutputShaft : MonoBehaviour, IItemHolder
{
    [Header("State")]
    public ItemDefinition CurrentItem { get; private set; }

    [Header("Visuals")]
    [SerializeField] private Transform _spawnPoint;

    private GameObject _currentVisualObj;

    public bool TryDeposit(ItemDefinition item)
    {
        if (CurrentItem != null) return false;

        CurrentItem = item;
        SpawnVisuals();
        return true;
    }

    // Interface Implementation: Das Item ruft das auf
    public bool TryTakeItem(WorldItem itemScript)
    {
        // Sicherheitscheck: Ist das wirklich unser Item?
        if (_currentVisualObj != itemScript.gameObject) return false;

        // Item logisch entfernen
        CurrentItem = null;
        _currentVisualObj = null; // Referenz löschen, das Destroy macht das Item selbst

        return true; // "Ja, du darfst gehen"
    }

    private void SpawnVisuals()
    {
        if (CurrentItem != null && CurrentItem.VisualPrefab != null)
        {
            _currentVisualObj = Instantiate(CurrentItem.VisualPrefab, _spawnPoint);
            _currentVisualObj.transform.localPosition = Vector3.zero;
            _currentVisualObj.transform.localRotation = Quaternion.identity;

            // WICHTIG: Wir kleben das WorldItem Script drauf und initialisieren es
            WorldItem worldItem = _currentVisualObj.AddComponent<WorldItem>();

            // Falls das Prefab noch keinen Collider hat, fügen wir einen hinzu
            if (!_currentVisualObj.TryGetComponent<Collider>(out var col))
            {
                BoxCollider box = _currentVisualObj.AddComponent<BoxCollider>();
                box.size = Vector3.one * 0.5f; // Standardgröße
            }

            // Verbindung herstellen: Item kennt jetzt den Schacht
            worldItem.Initialize(CurrentItem, this);
        }
    }
}