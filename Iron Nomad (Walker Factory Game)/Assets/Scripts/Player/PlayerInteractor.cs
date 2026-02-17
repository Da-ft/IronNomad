using UnityEngine;
using IronNomad.Inputs;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float _interactionRange = 3f;
    [SerializeField] private LayerMask _interactionLayer;

    [Header("Dependencies")]
    [SerializeField] private Transform _cameraRoot;
    [SerializeField] private InputReader _inputReader;

    private InventorySystem _inventory;

    private void Awake()
    {
        _inventory = GetComponent<InventorySystem>();
    }

    private void OnEnable() => _inputReader.InteractEvent += TryInteract;
    private void OnDisable() => _inputReader.InteractEvent -= TryInteract;

    private void TryInteract()
    {
        Ray ray = new Ray(_cameraRoot.position, _cameraRoot.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _interactionRange, _interactionLayer))
        {
            // Check auf WorldItem (Sachen am Boden)
            WorldItem worldItem = hit.collider.GetComponent<WorldItem>();
            // Fallback: Check im Parent (oft ist Collider auf Child)
            if (worldItem == null) worldItem = hit.collider.GetComponentInParent<WorldItem>();

            if (worldItem != null)
            {
                // Versuch es ins Inventar zu packen
                if (_inventory.AddItem(worldItem.Definition))
                {
                    // Erfolgreich aufgenommen -> Zerstören via Interface logic
                    worldItem.PickUp(); // Ruft Destroy auf und meldet sich beim Holder ab
                    Debug.Log($"Item {worldItem.Definition.Name} aufgenommen!");
                }
                else
                {
                    Debug.Log("Inventar voll!");
                }
                return; // Interaction done
            }
            // TODO: GUI FÜR MASCHINEN HINZUFÜGEN
        }
    }
}