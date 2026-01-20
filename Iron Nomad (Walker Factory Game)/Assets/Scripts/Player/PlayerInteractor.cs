using UnityEngine;
using IronNomad.Inputs;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _camTransform;
    [SerializeField] private float _range = 3f;
    [SerializeField] private LayerMask _interactLayer;

    private void OnEnable() => _inputReader.InteractEvent += TryInteract;
    private void OnDisable() => _inputReader.InteractEvent -= TryInteract;

    private void TryInteract()
    {
        Ray ray = new Ray(_camTransform.position, _camTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _range, _interactLayer))
        {
            // search for interface (item, machine or obj)
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                interactable.OnInteract();
            }

            // Search for Parent
            else if (hit.collider.GetComponentInParent<IInteractable>() is IInteractable parentInteractable)
            {
                parentInteractable.OnInteract();
            }
        }
    }
}