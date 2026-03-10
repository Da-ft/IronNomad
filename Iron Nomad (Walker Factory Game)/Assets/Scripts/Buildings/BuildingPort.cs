using UnityEngine;

public enum PortType { Input, Output }

[System.Serializable]
public class BuildingPort
{
    [Tooltip("Eindeutiger Name des Ports, z.B. 'Input Left' oder 'Output Right'")]
    public string Name;

    [Tooltip("Input = nimmt Items entgegen, Output = gibt Items weiter")]
    public PortType Type;

    [Tooltip("Richtung relativ zum Gebäude im Grid. (0,1)=vorne, (0,-1)=hinten, (-1,0)=links, (1,0)=rechts")]
    public Vector2Int Direction;

    [Tooltip("Verschiebung entlang der Gebäudekante in Grid-Zellen. " +
             "0 = mittig, -1 = eine Zelle links, 1 = eine Zelle rechts (relativ zur Port-Richtung)")]
    public int EdgeOffset = 0;

    /// <summary>
    /// Gibt die rotierte Port-Richtung zurück.
    /// </summary>
    public Vector2Int GetRotatedDirection(int rotationDegrees)
    {
        return RotateVector(Direction, rotationDegrees);
    }

    /// <summary>
    /// Gibt den rotierten EdgeOffset-Vektor zurück.
    /// Perpendicular zu Direction, rotiert mit dem Gebäude.
    /// </summary>
    public Vector2Int GetRotatedEdgeOffset(int rotationDegrees)
    {
        // Senkrecht zu Direction: (x,y) → (-y, x)
        Vector2Int perp = new Vector2Int(-Direction.y, Direction.x);
        return RotateVector(perp, rotationDegrees) * EdgeOffset;
    }

    private static Vector2Int RotateVector(Vector2Int v, int rotationDegrees)
    {
        return rotationDegrees switch
        {
            90 => new Vector2Int(v.y, -v.x),
            180 => new Vector2Int(-v.x, -v.y),
            270 => new Vector2Int(-v.y, v.x),
            _ => v
        };
    }
}