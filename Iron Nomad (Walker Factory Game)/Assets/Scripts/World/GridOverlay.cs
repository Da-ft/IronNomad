using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zeigt das Grid als Linienmesh an wenn der Spieler baut.
/// Auf dasselbe GameObject wie WalkerGrid legen.
/// </summary>
[RequireComponent(typeof(WalkerGrid))]
public class GridOverlay : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Material _lineMaterial;
    [SerializeField] private Color _freeColor = new Color(1f, 1f, 1f, 0.25f);
    [SerializeField] private Color _occupiedColor = new Color(1f, 0.3f, 0.3f, 0.4f);
    [SerializeField] private float _lineHeight = 0.02f;

    private WalkerGrid _grid;
    private GameObject _overlayRoot;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private bool _dirty = true;

    private void Awake()
    {
        _grid = GetComponent<WalkerGrid>();

        // Eigenes Child-GameObject für das Mesh
        _overlayRoot = new GameObject("GridOverlayMesh");
        _overlayRoot.transform.SetParent(transform, false);

        _meshFilter = _overlayRoot.AddComponent<MeshFilter>();
        _meshRenderer = _overlayRoot.AddComponent<MeshRenderer>();
        _meshRenderer.material = _lineMaterial;
        _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _meshRenderer.receiveShadows = false;

        _overlayRoot.SetActive(false);
    }

    private void OnEnable()
    {
        InputEvents.OnBuildMode += OnBuildModeToggled;
        InputEvents.OnCloseMenu += Hide;
    }

    private void OnDisable()
    {
        InputEvents.OnBuildMode -= OnBuildModeToggled;
        InputEvents.OnCloseMenu -= Hide;
    }

    private void OnBuildModeToggled()
    {
        if (_overlayRoot.activeSelf) Hide();
        else Show();
    }

    public void Show()
    {
        _overlayRoot.SetActive(true);
        if (_dirty) RebuildMesh();
    }

    public void Hide()
    {
        _overlayRoot.SetActive(false);
    }

    /// <summary>Aufrufen wenn sich belegte Zellen geändert haben.</summary>
    public void MarkDirty() => _dirty = true;

    // --- Mesh aufbauen ---

    private void RebuildMesh()
    {
        _dirty = false;
        float size = _grid.CellSize;
        float half = size * 0.5f;

        var occupied = new HashSet<Vector2Int>(_grid.OccupiedCells);

        var vertices = new List<Vector3>();
        var colors = new List<Color>();
        var triangles = new List<int>();

        foreach (var cell in _grid.AvailableCells)
        {
            Color c = occupied.Contains(cell) ? _occupiedColor : _freeColor;
            AddCellQuad(cell, size, half, c, vertices, colors, triangles);
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();

        _meshFilter.mesh = mesh;
    }

    private void AddCellQuad(
        Vector2Int cell, float size, float half, Color color,
        List<Vector3> verts, List<Color> colors, List<int> tris)
    {
        float cx = cell.x * size;
        float cz = cell.y * size;
        float y = _lineHeight;

        // Rahmen als 4 dünne Quads (je eine Kante)
        float t = size * 0.04f; // Liniendicke = 4% der Zellgröße

        int i = verts.Count;

        // Unterkante
        AddQuadVerts(new Vector3(cx - half, y, cz - half),
                     new Vector3(cx + half, y, cz - half),
                     new Vector3(cx + half, y, cz - half + t),
                     new Vector3(cx - half, y, cz - half + t),
                     color, verts, colors, tris);

        // Oberkante
        AddQuadVerts(new Vector3(cx - half, y, cz + half - t),
                     new Vector3(cx + half, y, cz + half - t),
                     new Vector3(cx + half, y, cz + half),
                     new Vector3(cx - half, y, cz + half),
                     color, verts, colors, tris);

        // Linke Kante
        AddQuadVerts(new Vector3(cx - half, y, cz - half),
                     new Vector3(cx - half + t, y, cz - half),
                     new Vector3(cx - half + t, y, cz + half),
                     new Vector3(cx - half, y, cz + half),
                     color, verts, colors, tris);

        // Rechte Kante
        AddQuadVerts(new Vector3(cx + half - t, y, cz - half),
                     new Vector3(cx + half, y, cz - half),
                     new Vector3(cx + half, y, cz + half),
                     new Vector3(cx + half - t, y, cz + half),
                     color, verts, colors, tris);
    }

    private void AddQuadVerts(
        Vector3 a, Vector3 b, Vector3 c, Vector3 d,
        Color color,
        List<Vector3> verts, List<Color> colors, List<int> tris)
    {
        int i = verts.Count;
        verts.Add(a); verts.Add(b); verts.Add(c); verts.Add(d);
        colors.Add(color); colors.Add(color); colors.Add(color); colors.Add(color);
        tris.Add(i); tris.Add(i + 2); tris.Add(i + 1);
        tris.Add(i); tris.Add(i + 3); tris.Add(i + 2);
    }
}