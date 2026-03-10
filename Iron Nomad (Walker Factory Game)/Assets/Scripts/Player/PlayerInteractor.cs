using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float _interactionRange = 3f;
    [SerializeField] private float _interactionRadius = 0.3f;
    [SerializeField] private LayerMask _interactionLayer;

    [Header("Dependencies")]
    [SerializeField] private Transform _cameraRoot;

    private InventorySystem _inventory;

    private void Awake() => _inventory = GetComponent<InventorySystem>();

    private void OnEnable() => InputEvents.OnInteract += TryInteract;
    private void OnDisable() => InputEvents.OnInteract -= TryInteract;

    private void TryInteract()
    {
        Ray ray = new Ray(_cameraRoot.position, _cameraRoot.forward);
        if (Physics.SphereCast(ray, _interactionRadius, out RaycastHit hit, _interactionRange, _interactionLayer))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            interactable?.OnInteract(_inventory);
        }
    }
}