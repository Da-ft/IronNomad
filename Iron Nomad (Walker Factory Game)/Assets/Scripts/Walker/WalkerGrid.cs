using UnityEngine;

public class WalkerGrid : MonoBehaviour
{
    [SerializeField] private float _cellSize = 2f;

    public Vector3 GetNearestGridPoint(Vector3 worldPosition)
    {
        // 1. Welt-Position in Lokale Position des Walkers umrechnen
        // Das ist wichtig, weil der Walker sich dreht und bewegt!
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);

        // 2. Auf das Raster runden
        float x = Mathf.Round(localPos.x / _cellSize) * _cellSize;
        float z = Mathf.Round(localPos.z / _cellSize) * _cellSize;

        // Wir setzen Y fix auf 0 (oder _cellSize/2), damit es AUF der Oberfläche sitzt
        // Hier gehen wir davon aus, dass der Pivot des Gebäudes unten mittig ist.
        float y = _cellSize / 0.04f;

        // 3. Zurück in Welt-Koordinaten rechnen
        return transform.TransformPoint(new Vector3(x, y, z));
    }

    // Only for Editor
    private void OnDrawGizmos()
    {
        // Farbe des Gitters
        Gizmos.color = new Color(0, 1, 1, 0.3f); // Cyan, halbtransparent

        // Wir nutzen die Matrix des Walkers, damit sich die Gizmos mitdrehen!
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        // Wie groß soll das gezeichnete Feld sein? (z.B. 20x20 Meter)
        int width = 10;
        int length = 10;

        // Zeichne kleine Punkte oder Würfel für jeden Slot
        for (int x = -width; x <= width; x++)
        {
            for (int z = -length; z <= length; z++)
            {
                // Position im lokalen Space berechnen
                Vector3 pos = new Vector3(x * _cellSize, 0, z * _cellSize);

                // Kleinen Würfel zeichnen (stellt einen Bauplatz dar)
                Gizmos.DrawWireCube(pos, new Vector3(_cellSize, 0.1f, _cellSize) * 0.9f);
            }
        }

        // Matrix zurücksetzen (Sauberkeit muss sein)
        Gizmos.matrix = oldMatrix;
    }
}