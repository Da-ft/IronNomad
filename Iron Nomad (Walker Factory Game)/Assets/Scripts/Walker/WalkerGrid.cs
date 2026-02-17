using System.Collections.Generic;
using UnityEngine;

public class WalkerGrid : MonoBehaviour
{
    [SerializeField] private float _cellSize = 2f;
    public float CellSize => _cellSize;

    // Datenbank: Koordinate -> Bauteil
    private Dictionary<Vector2Int, IItemHolder> _gridObjects = new Dictionary<Vector2Int, IItemHolder>();

    // Objekt Registrieren (wird vom Gebäude beim Bau aufgerufen)
    public void RegisterObject(Vector3 worldPos, IItemHolder holder)
    {
        Vector2Int coords = WorldToGridCoords(worldPos);
        if (_gridObjects.ContainsKey(coords))
        {
            Debug.LogWarning($"Grid-Konflikt auf {coords}! Überschrieben.");
            _gridObjects[coords] = holder;
        }
        else
        {
            _gridObjects.Add(coords, holder);
        }
    }

    // Objekt austragen (beim abreißen)
    public void UnregisterObject(Vector3 worldPos)
    {
        Vector2Int coords = WorldToGridCoords(worldPos);
        if (_gridObjects.ContainsKey(coords)) _gridObjects.Remove(coords);
    }

    // Nachbar abfragen
    public IItemHolder GetHolderAt(Vector3 worldPos)
    {
        Vector2Int coords = WorldToGridCoords(worldPos);
        return _gridObjects.TryGetValue(coords, out IItemHolder holder) ? holder : null;
    }

    // Helper Method für World -> Grid Coords
    public Vector2Int WorldToGridCoords(Vector3 worldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        int x = Mathf.RoundToInt(localPos.x / _cellSize);
        int z = Mathf.RoundToInt(localPos.z / _cellSize);
        return new Vector2Int(x, z);
    }

    // Helper Method für Grid -> Welt (für snapping)
    public Vector3 GetNearestGridPoint(Vector3 worldPosition)
    {
        Vector2Int coords = WorldToGridCoords(worldPosition);
        Vector3 localSnaped = new Vector3(coords.x * _cellSize, 0, coords.y * _cellSize);
        return transform.TransformPoint(localSnaped);
    }

    // Gizmos zur Visualisierung belegter Felder
    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        foreach (var kvp in _gridObjects)
        {
            Gizmos.color = Color.green;
            Vector3 pos = new Vector3(kvp.Key.x * _cellSize, 0.5f, kvp.Key.y * _cellSize);
            Gizmos.DrawWireCube(pos, Vector3.one * _cellSize * 0.8f);
        }
    }
}