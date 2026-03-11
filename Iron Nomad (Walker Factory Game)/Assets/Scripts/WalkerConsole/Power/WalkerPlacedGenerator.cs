using UnityEngine;

/// <summary>
/// Speichert was auf welcher Gridzelle platziert wurde.
/// Kein Prefab — nur Datenhaltung für das UI.
/// </summary>
[System.Serializable]
public class WalkerPlacedGenerator
{
    public BuildingDefinition Definition;
    public Vector2Int Origin;   // Zelle oben-links

    public WalkerPlacedGenerator(BuildingDefinition def, Vector2Int origin)
    {
        Definition = def;
        Origin = origin;
    }

    /// <summary>Alle Zellen die dieses Gebäude belegt.</summary>
    public bool OccupiesCell(Vector2Int cell)
    {
        Vector2Int size = Definition.GridSize;
        return cell.x >= Origin.x && cell.x < Origin.x + size.x
            && cell.y >= Origin.y && cell.y < Origin.y + size.y;
    }
}