using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IronNomad.Inputs;

public class TechTreeMenuUI : MonoBehaviour, IMenu
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private List<BuildingDefinition> _allBuildings;

    [Header("UI References")]
    [SerializeField] private GameObject _menuRoot;
    [SerializeField] private RectTransform _graphContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject _nodePrefab;
    [SerializeField] private GameObject _linePrefab; // Einfaches Image (1px hoch, weiß)

    [Header("Layout Config")]
    [SerializeField] private float _nodeWidth = 200f;
    [SerializeField] private float _nodeHeight = 120f;
    [SerializeField] private float _horizontalSpacing = 280f;
    [SerializeField] private float _verticalSpacing = 160f;

    public bool IsOpen => _isOpen;
    private bool _isOpen = false;

    private Dictionary<BuildingDefinition, TechTreeNodeUI> _nodes = new();
    private List<GameObject> _lines = new();

    private void Start()
    {
        UIManager.Instance.RegisterMenu(this);
        _menuRoot.SetActive(false);
        BuildGraph();
    }

    private void OnDestroy()
    {
        UIManager.Instance?.UnregisterMenu(this);
    }

    // --- IMenu ---

    public void Open()
    {
        _isOpen = true;
        _menuRoot.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _inputReader.DisableGameplay();
        _inputReader.ResetLook();
        _inputReader.ResetMove();
    }

    public void Close()
    {
        _isOpen = false;
        _menuRoot.SetActive(false);
    }

    // --- Graph ---

    private void BuildGraph()
    {
        Dictionary<BuildingDefinition, int> depths = CalculateDepths();
        Dictionary<int, List<BuildingDefinition>> columns = GroupByDepth(depths);

        // Nodes spawnen und positionieren
        foreach (var (depth, buildings) in columns)
        {
            int count = buildings.Count;
            for (int i = 0; i < count; i++)
            {
                BuildingDefinition building = buildings[i];

                float x = depth * _horizontalSpacing;
                float y = (i - (count - 1) / 2f) * -_verticalSpacing;

                GameObject obj = Instantiate(_nodePrefab, _graphContainer);
                RectTransform rt = obj.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(x, y);
                rt.sizeDelta = new Vector2(_nodeWidth, _nodeHeight);

                TechTreeNodeUI node = obj.GetComponent<TechTreeNodeUI>();
                node.Setup(building);
                _nodes[building] = node;
            }
        }

        // Verbindungslinien zeichnen (nach dem nächsten Frame damit Positionen stimmen)
        StartCoroutine(DrawLinesNextFrame());
    }

    private System.Collections.IEnumerator DrawLinesNextFrame()
    {
        yield return null;
        DrawLines();
    }

    private void DrawLines()
    {
        foreach (var (building, node) in _nodes)
        {
            foreach (var req in building.BuildingRequirements)
            {
                if (!_nodes.TryGetValue(req, out TechTreeNodeUI fromNode)) continue;
                DrawLine(fromNode.GetComponent<RectTransform>(), node.GetComponent<RectTransform>());
            }
        }
    }

    private void DrawLine(RectTransform from, RectTransform to)
    {
        GameObject lineObj = Instantiate(_linePrefab, _graphContainer);
        lineObj.transform.SetAsFirstSibling(); // Linien hinter Nodes

        RectTransform line = lineObj.GetComponent<RectTransform>();

        Vector2 fromPos = from.anchoredPosition;
        Vector2 toPos = to.anchoredPosition;

        Vector2 dir = toPos - fromPos;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        line.anchoredPosition = fromPos + dir * 0.5f;
        line.sizeDelta = new Vector2(distance, 3f);
        line.localRotation = Quaternion.Euler(0, 0, angle);

        _lines.Add(lineObj);
    }

    // --- Layout Helpers ---

    // Berechnet die Tiefe jedes Gebäudes im Graphen (0 = kein Requirement)
    private Dictionary<BuildingDefinition, int> CalculateDepths()
    {
        Dictionary<BuildingDefinition, int> depths = new();

        foreach (var building in _allBuildings)
            CalculateDepth(building, depths, new HashSet<BuildingDefinition>());

        return depths;
    }

    private int CalculateDepth(BuildingDefinition building, Dictionary<BuildingDefinition, int> depths, HashSet<BuildingDefinition> visited)
    {
        if (depths.TryGetValue(building, out int cached)) return cached;
        if (visited.Contains(building)) return 0; // Zirkuläre Abhängigkeit verhindern

        visited.Add(building);

        int maxDepth = 0;
        foreach (var req in building.BuildingRequirements)
        {
            int d = CalculateDepth(req, depths, visited) + 1;
            if (d > maxDepth) maxDepth = d;
        }

        depths[building] = maxDepth;
        return maxDepth;
    }

    private Dictionary<int, List<BuildingDefinition>> GroupByDepth(Dictionary<BuildingDefinition, int> depths)
    {
        Dictionary<int, List<BuildingDefinition>> columns = new();

        foreach (var (building, depth) in depths)
        {
            if (!columns.ContainsKey(depth))
                columns[depth] = new List<BuildingDefinition>();
            columns[depth].Add(building);
        }

        return columns;
    }
}