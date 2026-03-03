using UnityEngine;
using IronNomad.Inputs;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float _interactionRange = 3f;
    [SerializeField] private float _interactionRadius = 0.3f; // SphereCast Radius
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

        // SphereCast statt Raycast - verzeiht ungenaues Anvisieren
        if (Physics.SphereCast(ray, _interactionRadius, out RaycastHit hit, _interactionRange, _interactionLayer))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
                interactable.OnInteract(_inventory);
        }
    }
}