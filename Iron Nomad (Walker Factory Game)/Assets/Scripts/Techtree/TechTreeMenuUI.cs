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

    public bool IsOpen => _isOpen;
    private bool _isOpen = false;

    private Dictionary<BuildingDefinition, TechTreeNodeUI> _nodes = new();
    private List<GameObject> _lines = new();

    private void Awake()
    {
        _allBuildings = new List<BuildingDefinition>(Resources.LoadAll<BuildingDefinition>("Buildings"));

    }
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

    public void Open()
    {
        _isOpen = true;
        _menuRoot.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _inputReader.DisableGameplay();
        _inputReader.ResetLook();
        _inputReader.ResetMove();
        _inputReader.ScrollEvent += HandleZoom;
    }

    public void Close()
    {
        _isOpen = false;
        _menuRoot.SetActive(false);
        _inputReader.ScrollEvent -= HandleZoom;
    }

    private void HandleZoom(float direction)
    {
        float currentScale = _graphContainer.localScale.x;
        float newScale = Mathf.Clamp(currentScale + direction * _zoomStep, _zoomMin, _zoomMax);
        if (Mathf.Approximately(currentScale, newScale)) return;

        // Mausposition relativ zum GraphContainer vor dem Zoom
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _graphContainer,
            UnityEngine.InputSystem.Mouse.current.position.ReadValue(),
            null,
            out Vector2 localMousePos
        );

        // Skalieren
        _graphContainer.localScale = Vector3.one * newScale;

        // Position verschieben damit der Punkt unter der Maus stabil bleibt
        float scaleDelta = newScale - currentScale;
        _graphContainer.anchoredPosition -= localMousePos * scaleDelta;
    }

    private void BuildGraph()
    {
        Dictionary<BuildingDefinition, int> depths = CalculateDepths();
        Dictionary<int, List<BuildingDefinition>> rows = GroupByDepth(depths);

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
        lineObj.transform.SetAsFirstSibling();

        RectTransform line = lineObj.GetComponent<RectTransform>();
        Vector2 fromPos = from.anchoredPosition + new Vector2(0, -_nodeHeight / 2f);
        Vector2 toPos = to.anchoredPosition + new Vector2(0, _nodeHeight / 2f);

        Vector2 dir = toPos - fromPos;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        line.anchoredPosition = fromPos + dir * 0.5f;
        line.sizeDelta = new Vector2(distance, 3f);
        line.localRotation = Quaternion.Euler(0, 0, angle);

        _lines.Add(lineObj);
    }

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
        if (visited.Contains(building)) return 0;

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
        Dictionary<int, List<BuildingDefinition>> rows = new();
        foreach (var (building, depth) in depths)
        {
            if (!rows.ContainsKey(depth))
                rows[depth] = new List<BuildingDefinition>();
            rows[depth].Add(building);
        }
        return rows;
    }
}