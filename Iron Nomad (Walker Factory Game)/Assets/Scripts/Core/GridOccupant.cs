using UnityEngine;

public class GridOccupant : MonoBehaviour
{
    private WalkerGrid _grid;
    private IItemHolder _myHolder;

    private void Start()
    {
        // 1. Suche nach dem Interface auf DIESEM Objekt
        _myHolder = GetComponent<IItemHolder>();
        if (_myHolder == null)
        {
            Debug.LogError($"FEHLER: Objekt '{gameObject.name}' hat GridOccupant, aber kein Script, das IItemHolder implementiert (z.B. ConveyorBelt oder OutputShaft)!");
            return;
        }

        // 2. Suche nach dem Grid im PARENT (Walker)
        _grid = GetComponentInParent<WalkerGrid>();
        if (_grid == null)
        {
            Debug.LogError($"FEHLER: Objekt '{gameObject.name}' findet kein WalkerGrid in seinen Eltern! Bitte zieh es in der Hierarchy unter den Walker.");
            return;
        }

        // Wenn beides da ist -> Anmelden
        _grid.RegisterObject(transform.position, _myHolder);
    }

    private void OnDestroy()
    {
        if (_grid != null)
        {
            _grid.UnregisterObject(transform.position);
        }
    }
}