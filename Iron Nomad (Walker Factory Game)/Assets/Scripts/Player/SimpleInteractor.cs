using UnityEngine;
using IronNomad.Inputs;

public class SimpleInteractor : MonoBehaviour
{
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _camTransform;
    [SerializeField] private float _range = 3f;
    [SerializeField] private LayerMask _interactionLayer; // Mach nen neuen Layer "Interactable"

    private void OnEnable() => _inputReader.FireEvent += OnInteract; // Wir nutzen erstmal Linksklick (Fire) auch hierfür
    private void OnDisable() => _inputReader.FireEvent -= OnInteract;

    private void OnInteract()
    {
        Ray ray = new Ray(_camTransform.position, _camTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _range, _interactionLayer))
        {
            // Wir suchen jetzt nach dem WorldItem Script am getroffenen Objekt
            if (hit.collider.TryGetComponent(out WorldItem item))
            {
                ItemDefinition data = item.PickUp(); // Versuch es aufzuheben

                if (data != null)
                {
                    Debug.Log($"SPIELER: Habe {data.Name} direkt vom Band/Schacht gegriffen!");
                    // Hier später: Inventory.Add(data);
                }
            }
        }
    }
}
