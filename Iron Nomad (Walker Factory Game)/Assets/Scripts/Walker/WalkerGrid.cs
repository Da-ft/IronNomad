using System.Collections.Generic;
using UnityEngine;

public class WalkerGrid : MonoBehaviour
{
    [SerializeField] private float _cellSize = 2f;
    public float CellSize => _cellSize;

    // Alle verfügbaren Zellen (begehbare Fläche)
    private HashSet<Vector2Int> _availableCells = new HashSet<Vector2Int>();

    // Belegte Zellen (durch BuilderTool gesetzt)
    private HashSet<Vector2Int> _occupiedCells = new HashSet<Vector2Int>();

    // Item-Pipeline: Koordinate -> IItemHolder (Maschinen)
    private Dictionary<Vector2Int, IItemHolder> _gridObjects = new Dictionary<Vector2Int, IItemHolder>();

    private void Start()
    {
        ScanGridSurfaces();
    }

    private void ScanGridSurfaces()
    {
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (!child.CompareTag("Grid")) continue;

            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer == null) continue;

            Bounds bounds = renderer.bounds;
            Vector3 min = transform.InverseTransformPoint(bounds.min);
            Vector3 max = transform.InverseTransformPoint(bounds.max);

            int minX = Mathf.RoundToInt(min.x / _cellSize);
            int maxX = Mathf.RoundToInt(max.x / _cellSize);
            int minZ = Mathf.RoundToInt(min.z / _cellSize);
            int maxZ = Mathf.RoundToInt(max.z / _cellSize);

            for (int x = minX; x <= maxX; x++)
                for (int z = minZ; z <= maxZ; z++)
                    _availableCells.Add(new Vector2Int(x, z));
        }

        Debug.Log($"WalkerGrid: {_availableCells.Count} Zellen gefunden.");
    }

    // --- Zellen-Belegung (BuilderTool) ---

    public void OccupyCell(Vector3 worldPos)
    {
        Vector2Int coords = WorldToGridCoords(worldPos);
        if (!_availableCells.Contains(coords))
        {
            Debug.LogWarning($"OccupyCell: Zelle {coords} ist nicht verfügbar!");
            return;
        }
        _occupiedCells.Add(coords);
    }

    public void FreeCell(Vector3 worldPos)
    {
        _occupiedCells.Remove(WorldToGridCoords(worldPos));
    }

    public bool IsCellAvailable(Vector2Int coords)
    {
        return _availableCells.Contains(coords)
            && !_occupiedCells.Contains(coords);
    }

    public bool IsCellAvailable(Vector3 worldPos)
    {
        return IsCellAvailable(WorldToGridCoords(worldPos));
    }

    // --- Item-Pipeline (BaseGridMachine) ---

    public void RegisterObject(Vector3 worldPos, IItemHolder holder)
    {
        Vector2Int coords = WorldToGridCoords(worldPos);
        if (_gridObjects.ContainsKey(coords))
            Debug.LogWarning($"Grid-Konflikt auf {coords}! Überschrieben.");
        _gridObjects[coords] = holder;
    }

    public void UnregisterObject(Vector3 worldPos)
    {
        _gridObjects.Remove(WorldToGridCoords(worldPos));
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
        foreach (var coords in _occupiedCells)
        {
            Vector3 pos = new Vector3(coords.x * _cellSize, 0.1f, coords.y * _cellSize);
            Gizmos.DrawWireCube(pos, Vector3.one * _cellSize * 0.8f);
        }
    }
}