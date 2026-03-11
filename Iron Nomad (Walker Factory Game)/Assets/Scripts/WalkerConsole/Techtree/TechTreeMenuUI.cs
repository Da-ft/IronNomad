using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reines Sub-Panel — kein IMenu, keine UIManager-Registrierung.
/// Wird von WalkerConsoleUI ein/ausgeblendet.
/// </summary>
public class TechTreeMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform _graphContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject _nodePrefab;
    [SerializeField] private GameObject _linePrefab;

    [Header("Layout Config")]
    [SerializeField] private float _nodeWidth = 200f;
    [SerializeField] private float _nodeHeight = 120f;
    [SerializeField] private float _horizontalSpacing = 280f;
    [SerializeField] private float _verticalSpacing = 200f;

    [Header("Zoom")]
    [SerializeField] private float _zoomMin = 0.3f;
    [SerializeField] private float _zoomMax = 1f;
    [SerializeField] private float _zoomStep = 0.1f;

    private List<BuildingDefinition> _allBuildings;
    private Dictionary<BuildingDefinition, TechTreeNodeUI> _nodes = new();
    private List<GameObject> _lines = new();
    private bool _built = false;

    private void Awake()
    {
        _allBuildings = new List<BuildingDefinition>(
            Resources.LoadAll<BuildingDefinition>("Buildings"));
    }

    /// <summary>Von WalkerConsoleUI aufgerufen wenn dieser Tab aktiv wird.</summary>
    public void OnTabOpened()
    {
        InputEvents.OnScroll += HandleZoom;
        if (!_built) { BuildGraph(); _built = true; }
    }

    /// <summary>Von WalkerConsoleUI aufgerufen wenn dieser Tab verlassen wird.</summary>
    public void OnTabClosed()
    {
        InputEvents.OnScroll -= HandleZoom;
    }

    // --- Zoom ---

    private void HandleZoom(float direction)
    {
        float current = _graphContainer.localScale.x;
        float newScale = Mathf.Clamp(current + direction * _zoomStep, _zoomMin, _zoomMax);
        if (Mathf.Approximately(current, newScale)) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _graphContainer,
            UnityEngine.InputSystem.Mouse.current.position.ReadValue(),
            null, out Vector2 localMousePos);

        _graphContainer.localScale = Vector3.one * newScale;
        _graphContainer.anchoredPosition -= localMousePos * (newScale - current);
    }

    // --- Graph ---

    private void BuildGraph()
    {
        var depths = CalculateDepths();
        var rows = GroupByDepth(depths);

        foreach (var (depth, buildings) in rows)
        {
            int count = buildings.Count;
            for (int i = 0; i < count; i++)
            {
                BuildingDefinition building = buildings[i];
                float x = (i - (count - 1) / 2f) * _horizontalSpacing;
                float y = -depth * _verticalSpacing;

                GameObject obj = Instantiate(_nodePrefab, _graphContainer);
                RectTransform rt = obj.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(x, y);
                rt.sizeDelta = new Vector2(_nodeWidth, _nodeHeight);

                TechTreeNodeUI node = obj.GetComponent<TechTreeNodeUI>();
                node.Setup(building);
                _nodes[building] = node;
            }
        }

        Canvas.ForceUpdateCanvases();
        DrawLines();
    }

    private void DrawLines()
    {
        foreach (var (building, node) in _nodes)
            foreach (var req in building.BuildingRequirements)
                if (_nodes.TryGetValue(req, out TechTreeNodeUI fromNode))
                    DrawLine(fromNode.GetComponent<RectTransform>(),
                             node.GetComponent<RectTransform>());
    }

    private void DrawLine(RectTransform from, RectTransform to)
    {
        GameObject lineObj = Instantiate(_linePrefab, _graphContainer);
        lineObj.transform.SetAsFirstSibling();
        RectTransform line = lineObj.GetComponent<RectTransform>();

        Vector2 fromPos = from.anchoredPosition + new Vector2(0, -_nodeHeight / 2f);
        Vector2 toPos = to.anchoredPosition + new Vector2(0, _nodeHeight / 2f);
        Vector2 dir = toPos - fromPos;

        line.anchoredPosition = fromPos + dir * 0.5f;
        line.sizeDelta = new Vector2(dir.magnitude, 3f);
        line.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        _lines.Add(lineObj);
    }

    private Dictionary<BuildingDefinition, int> CalculateDepths()
    {
        var depths = new Dictionary<BuildingDefinition, int>();
        foreach (var b in _allBuildings)
            CalculateDepth(b, depths, new HashSet<BuildingDefinition>());
        return depths;
    }

    private int CalculateDepth(BuildingDefinition b,
        Dictionary<BuildingDefinition, int> depths,
        HashSet<BuildingDefinition> visited)
    {
        if (depths.TryGetValue(b, out int cached)) return cached;
        if (visited.Contains(b)) return 0;
        visited.Add(b);

        int max = 0;
        foreach (var req in b.BuildingRequirements)
        {
            int d = CalculateDepth(req, depths, visited) + 1;
            if (d > max) max = d;
        }
        depths[b] = max;
        return max;
    }

    private Dictionary<int, List<BuildingDefinition>> GroupByDepth(
        Dictionary<BuildingDefinition, int> depths)
    {
        var rows = new Dictionary<int, List<BuildingDefinition>>();
        foreach (var (b, depth) in depths)
        {
            if (!rows.ContainsKey(depth)) rows[depth] = new List<BuildingDefinition>();
            rows[depth].Add(b);
        }
        return rows;
    }
}