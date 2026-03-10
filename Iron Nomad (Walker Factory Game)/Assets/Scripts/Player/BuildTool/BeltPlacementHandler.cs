using System.Collections.Generic;
using UnityEngine;

public enum BeltSegmentType { Straight, Curve }

public class BeltSegmentData
{
    public Vector3 Start;
    public Vector3 End;
    public BeltSegmentType Type;

    public BeltSegmentData(Vector3 start, Vector3 end, BeltSegmentType type)
    {
        Start = start;
        End = end;
        Type = type;
    }
}

public class BeltPlacementHandler
{
    private readonly WalkerGrid _grid;
    private readonly BuildingDefinition _straightDefinition;
    private readonly BuildingDefinition _curveDefinition;
    private readonly Transform _cameraRoot;
    private readonly float _raycastRange;
    private readonly Material _ghostMaterial;

    private enum State { WaitingForStart, WaitingForEnd }
    private State _state = State.WaitingForStart;

    private Vector3 _startWorldPos;
    private List<GameObject> _ghostSegments = new();

    public BeltPlacementHandler(
        WalkerGrid grid,
        BuildingDefinition definition,
        Transform cameraRoot,
        float raycastRange,
        Material ghostMaterial)
    {
        _grid = grid;
        _cameraRoot = cameraRoot;
        _raycastRange = raycastRange;
        _ghostMaterial = ghostMaterial;

        if (definition is BeltDefinitionSet set)
        {
            _straightDefinition = set;
            _curveDefinition = set.CurveDefinition != null ? set.CurveDefinition : set;
        }
        else
        {
            _straightDefinition = definition;
            _curveDefinition = definition;
        }
    }

    // --- Tick ---

    public void Tick()
    {
        if (!TryGetGridPoint(out Vector3 hitPoint)) return;

        if (_state == State.WaitingForStart)
            UpdateGhostPreview(_startWorldPos: hitPoint, endPos: hitPoint, singleDot: true);
        else
            UpdateGhostPreview(_startWorldPos: _startWorldPos, endPos: hitPoint, singleDot: false);
    }

    // --- Input ---

    public void OnPlace()
    {
        if (!TryGetGridPoint(out Vector3 hitPoint)) return;

        if (_state == State.WaitingForStart)
        {
            _startWorldPos = hitPoint;
            _state = State.WaitingForEnd;
        }
        else
        {
            SpawnSegments(hitPoint);
            ClearGhosts();
            _startWorldPos = hitPoint;
        }
    }

    public void Cancel()
    {
        ClearGhosts();
        _state = State.WaitingForStart;
    }

    // --- Segment-Berechnung: eine Zelle pro Segment ---

    private List<BeltSegmentData> CalculateSegments(Vector3 start, Vector3 end)
    {
        var segments = new List<BeltSegmentData>();

        Vector2Int startCoord = _grid.WorldToGridCoords(start);
        Vector2Int endCoord = _grid.WorldToGridCoords(end);

        // Alle Gridzellen entlang des Pfades sammeln: erst X, dann Z
        var coords = new List<Vector2Int>();
        Vector2Int current = startCoord;

        // Schritt 1: X-Richtung bis endCoord.x
        int stepX = endCoord.x > startCoord.x ? 1 : -1;
        while (current.x != endCoord.x)
        {
            coords.Add(current);
            current.x += stepX;
        }

        // Schritt 2: Z-Richtung bis endCoord.y
        int stepZ = endCoord.y > startCoord.y ? 1 : -1;
        while (current.y != endCoord.y)
        {
            coords.Add(current);
            current.y += stepZ;
        }

        coords.Add(endCoord); // letzten Punkt hinzufügen

        // Aus Koordinaten Segmente bauen
        for (int i = 0; i < coords.Count - 1; i++)
        {
            Vector3 segStart = GridToWorld(coords[i]);
            Vector3 segEnd = GridToWorld(coords[i + 1]);

            // Kurve: wenn vorherige und nächste Richtung unterschiedlich sind (Ecke)
            bool isCurve = false;
            if (i > 0)
            {
                Vector2Int prevDir = coords[i] - coords[i - 1];
                Vector2Int nextDir = coords[i + 1] - coords[i];
                isCurve = prevDir != nextDir;
            }

            segments.Add(new BeltSegmentData(segStart, segEnd,
                isCurve ? BeltSegmentType.Curve : BeltSegmentType.Straight));
        }

        return segments;
    }

    private Vector3 GridToWorld(Vector2Int coord)
    {
        return _grid.GetNearestGridPoint(
            _grid.transform.TransformPoint(
                new Vector3(coord.x * _grid.CellSize, 0, coord.y * _grid.CellSize)));
    }

    // --- Spawning ---

    private void SpawnSegments(Vector3 endPos)
    {
        foreach (var seg in CalculateSegments(_startWorldPos, endPos))
            SpawnSegment(seg, isGhost: false);
    }

    private GameObject SpawnSegment(BeltSegmentData seg, bool isGhost)
    {
        BuildingDefinition def = seg.Type == BeltSegmentType.Curve
            ? _curveDefinition
            : _straightDefinition;

        if (def?.BuildingPrefab == null)
        {
            Debug.LogWarning($"[BeltPlacementHandler] Kein Prefab für {seg.Type}!");
            return null;
        }

        Vector3 dir = seg.End - seg.Start;
        Quaternion rotation = dir.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(dir.normalized)
            : Quaternion.identity;

        GameObject obj = Object.Instantiate(def.BuildingPrefab, seg.Start, rotation);
        obj.transform.SetParent(_grid.transform);

        ConveyorBelt belt = obj.GetComponent<ConveyorBelt>();
        if (belt != null)
            belt.InitializeEndpoints(seg.Start, seg.End);

        if (isGhost)
        {
            ApplyMaterial(obj, _ghostMaterial);
            foreach (var col in obj.GetComponentsInChildren<Collider>()) col.enabled = false;
            foreach (var mb in obj.GetComponentsInChildren<MonoBehaviour>()) mb.enabled = false;
        }
        else
        {
            _grid.OccupyCell(seg.Start, Vector2Int.one, 0);

            ConstructibleBuilding constructible = obj.GetComponent<ConstructibleBuilding>()
                ?? obj.AddComponent<ConstructibleBuilding>();
            constructible.Initialize(def, 0);
        }

        return obj;
    }

    // --- Ghost ---

    private void UpdateGhostPreview(Vector3 _startWorldPos, Vector3 endPos, bool singleDot)
    {
        ClearGhosts();

        if (singleDot)
        {
            GameObject ghost = SpawnSingleGhost(endPos);
            if (ghost != null) _ghostSegments.Add(ghost);
            return;
        }

        foreach (var seg in CalculateSegments(_startWorldPos, endPos))
        {
            GameObject ghost = SpawnSegment(seg, isGhost: true);
            if (ghost != null) _ghostSegments.Add(ghost);
        }
    }

    private GameObject SpawnSingleGhost(Vector3 pos)
    {
        if (_straightDefinition?.BuildingPrefab == null) return null;
        GameObject obj = Object.Instantiate(_straightDefinition.BuildingPrefab, pos, Quaternion.identity);
        ApplyMaterial(obj, _ghostMaterial);
        foreach (var col in obj.GetComponentsInChildren<Collider>()) col.enabled = false;
        foreach (var mb in obj.GetComponentsInChildren<MonoBehaviour>()) mb.enabled = false;
        return obj;
    }

    private void ClearGhosts()
    {
        foreach (var g in _ghostSegments)
            if (g != null) Object.Destroy(g);
        _ghostSegments.Clear();
    }

    private bool TryGetGridPoint(out Vector3 point)
    {
        point = Vector3.zero;
        Ray ray = new Ray(_cameraRoot.position, _cameraRoot.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, _raycastRange)) return false;
        if (!hit.collider.CompareTag("Grid")) return false;
        point = _grid.GetNearestGridPoint(hit.point);
        return true;
    }

    private void ApplyMaterial(GameObject obj, Material mat)
    {
        if (mat == null) return;
        foreach (var r in obj.GetComponentsInChildren<Renderer>())
        {
            var mats = new Material[r.materials.Length];
            for (int i = 0; i < mats.Length; i++) mats[i] = mat;
            r.materials = mats;
        }
    }
}