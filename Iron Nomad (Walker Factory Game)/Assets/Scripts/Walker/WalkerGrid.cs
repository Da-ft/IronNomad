using System.Collections.Generic;
using UnityEngine;

public class WalkerGrid : MonoBehaviour
{
    [SerializeField] private float _cellSize = 2f;
    public float CellSize => _cellSize;

    // Alle verfügbaren Zellen (begehbare Fläche)
    private HashSet<Vector2Int> _availableCells = new HashSet<Vector2Int>();

    // Belegte Zellen
    private Dictionary<Vector2Int, IItemHolder> _gridObjects = new Dictionary<Vector2Int, IItemHolder>();

    private void Start()
    {
        ScanGridSurfaces();
    }

    private void ScanGridSurfaces()
    {
        // Alle Children mit Tag "Grid" finden
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (!child.CompareTag("Grid")) continue;

            // Bounds der Plane im lokalen Walker-Raum berechnen
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer == null) continue;

            // Bounds in lokale Koordinaten umrechnen
            Bounds bounds = renderer.bounds;

            // Alle Zellen innerhalb der Bounds registrieren
            Vector3 min = transform.InverseTransformPoint(bounds.min);
            Vector3 max = transform.InverseTransformPoint(bounds.max);

            int minX = Mathf.RoundToInt(min.x / _cellSize);
            int maxX = Mathf.RoundToInt(max.x / _cellSize);
            int minZ = Mathf.RoundToInt(min.z / _cellSize);
            int maxZ = Mathf.RoundToInt(max.z / _cellSize);

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    _availableCells.Add(new Vector2Int(x, z));
                }
            }
            Debug.Log($"Plane: {child.name} | Min: {min} | Max: {max} | Zellen X: {minX}~{maxX} | Z: {minZ}~{maxZ}");

        }

        Debug.Log($"WalkerGrid: {_availableCells.Count} Zellen gefunden.");
    }

    // --- Public API ---

    public bool IsCellAvailable(Vector2Int coords)
    {
        return _availableCells.Contains(coords) && !_gridObjects.ContainsKey(coords);
    }

    public bool IsCellAvailable(Vector3 worldPos)
    {
        return IsCellAvailable(WorldToGridCoords(worldPos));
    }

    public void RegisterObject(Vector3 worldPos, IItemHolder holder)
    {
        Vector2Int coords = WorldToGridCoords(worldPos);

        if (!_availableCells.Contains(coords))
        {
            Debug.LogWarning($"'{gameObject.name}': Versuch auf ungültiger Zelle {coords} zu registrieren!");
            return;
        }

        if (_gridObjects.ContainsKey(coords))
        {
            Debug.LogWarning($"Grid-Konflikt auf {coords}! Überschrieben.");
        }

        _gridObjects[coords] = holder;
    }

    public void UnregisterObject(Vector3 worldPos)
    {
        Vector2Int coords = WorldToGridCoords(worldPos);
        _gridObjects.Remove(coords);
    }

    public IItemHolder GetHolderAt(Vector3 worldPos)
    {
        Vector2Int coords = WorldToGridCoords(worldPos);
        return _gridObjects.TryGetValue(coords, out IItemHolder holder) ? holder : null;
    }

    // --- Helper ---

    public Vector2Int WorldToGridCoords(Vector3 worldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        int x = Mathf.RoundToInt(localPos.x / _cellSize);
        int z = Mathf.RoundToInt(localPos.z / _cellSize);
        return new Vector2Int(x, z);
    }

    public Vector3 GetNearestGridPoint(Vector3 worldPos)
    {
        Vector2Int coords = WorldToGridCoords(worldPos);
        Vector3 localSnapped = new Vector3(coords.x * _cellSize, 0, coords.y * _cellSize);
        return transform.TransformPoint(localSnapped);
    }

    // --- Gizmos ---

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        // Verfügbare Zellen grau
        Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
        foreach (var cell in _availableCells)
        {
            Vector3 pos = new Vector3(cell.x * _cellSize, 0.05f, cell.y * _cellSize);
            Gizmos.DrawWireCube(pos, new Vector3(_cellSize, 0f, _cellSize));
        }

        // Belegte Zellen grün
        Gizmos.color = Color.green;
        foreach (var kvp in _gridObjects)
        {
            Vector3 pos = new Vector3(kvp.Key.x * _cellSize, 0.1f, kvp.Key.y * _cellSize);
            Gizmos.DrawWireCube(pos, Vector3.one * _cellSize * 0.8f);
        }
    }
}