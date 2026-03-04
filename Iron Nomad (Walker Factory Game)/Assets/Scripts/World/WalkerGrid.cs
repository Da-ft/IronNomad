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

    public void OccupyCell(Vector3 worldPos, Vector2Int gridSize, int rotation)
    {
        Vector2Int origin = WorldToGridCoords(worldPos);
        foreach (var cell in GetOccupiedCoords(origin, gridSize, rotation))
            _occupiedCells.Add(cell);
    }

    public void FreeCell(Vector3 worldPos, Vector2Int gridSize, int rotation)
    {
        Vector2Int origin = WorldToGridCoords(worldPos);
        foreach (var cell in GetOccupiedCoords(origin, gridSize, rotation))
            _occupiedCells.Remove(cell);
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

    public void RegisterObject(Vector3 worldPos, Vector2Int gridSize, int rotation, IItemHolder holder)
    {
        Vector2Int origin = WorldToGridCoords(worldPos);
        foreach (var cell in GetOccupiedCoords(origin, gridSize, rotation))
        {
            if (_gridObjects.ContainsKey(cell))
                Debug.LogWarning($"Grid-Konflikt auf {cell}! Überschrieben.");
            _gridObjects[cell] = holder;
        }
    }

    public void UnregisterObject(Vector3 worldPos, Vector2Int gridSize, int rotation)
    {
        Vector2Int origin = WorldToGridCoords(worldPos);
        foreach (var cell in GetOccupiedCoords(origin, gridSize, rotation))
            _gridObjects.Remove(cell);
    }

    public void RegisterObject(Vector3 worldPos, IItemHolder holder)
    => RegisterObject(worldPos, Vector2Int.one, 0, holder);

    public void UnregisterObject(Vector3 worldPos)
        => UnregisterObject(worldPos, Vector2Int.one, 0);

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

    public List<Vector2Int> GetOccupiedCoords(Vector2Int origin, Vector2Int gridSize, int rotation)
    {
        var cells = new List<Vector2Int>();

        // Offset damit das Gebäude zentriert ist
        int halfX = (gridSize.x - 1);
        int halfZ = (gridSize.y - 1);

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int z = 0; z < gridSize.y; z++)
            {
                int localX = x * 2 - halfX;
                int localZ = z * 2 - halfZ;

                // Rotation anwenden (0/90/180/270)
                Vector2Int rotated = RotateCoord(localX, localZ, rotation);
                cells.Add(origin + rotated / 2);
            }
        }
        return cells;
    }

    private Vector2Int RotateCoord(int x, int z, int rotation)
    {
        return rotation switch
        {
            90 => new Vector2Int(z, -x),
            180 => new Vector2Int(-x, -z),
            270 => new Vector2Int(-z, x),
            _ => new Vector2Int(x, z),
        };
    }

    public bool IsCellAvailable(Vector2Int origin, Vector2Int gridSize, int rotation)
    {
        foreach (var cell in GetOccupiedCoords(origin, gridSize, rotation))
        {
            if (!_availableCells.Contains(cell) || _occupiedCells.Contains(cell))
                return false;
        }
        return true;
    }

    public void FreeCell(Vector3 worldPos)
    {
        FreeCell(worldPos, Vector2Int.one, 0);
    }

    public Vector3 GetDirectionWorldPos(Vector3 origin, Vector2Int direction, int rotation)
    {
        Vector2Int rotated = RotateCoord(direction.x, direction.y, rotation);
        Vector3 offset = new Vector3(rotated.x, 0, rotated.y) * _cellSize;
        return origin + transform.TransformDirection(offset);
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