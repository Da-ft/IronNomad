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
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
                interactable.OnInteract(_inventory);
        }
    }
}