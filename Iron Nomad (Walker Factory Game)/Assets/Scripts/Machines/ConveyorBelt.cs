using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : BaseGridMachine, IInteractable
{
    [System.Serializable]
    public class BeltItem
    {
        public ItemDefinition Definition;
        public GameObject VisualObj;
        public float CurrentDistance;
    }

    [Header("Settings")]
    [SerializeField] protected float _itemsPerMinute = 60f;
    [SerializeField] protected float _itemSpacing = 0.6f;

    [Header("Visual Connections")]
    [SerializeField] protected Transform _startPoint;
    [SerializeField] protected Transform _endPoint;

    protected List<BeltItem> _items = new();
    protected float _beltLength;
    protected float _speed;

    private bool _initialized = false;

    // Gecachte Output-Port Referenz
    private BuildingPort _outputPort;

    protected override void Start()
    {
        base.Start();

        // Output-Port einmalig cachen
        if (_constructible?.Definition != null)
        {
            var outputs = _constructible.Definition.GetOutputPorts();
            if (outputs.Length > 0) _outputPort = outputs[0];
        }
    }

    private void Update()
    {
        if (!_initialized)
        {
            CalculateSpeed();
            _initialized = true;
        }

        MoveItems();
        CheckOutput();
    }

    protected virtual void CalculateSpeed()
    {
        if (_startPoint == null || _endPoint == null) return;
        _beltLength = Vector3.Distance(_startPoint.position, _endPoint.position);
        _speed = _beltLength / (60f / _itemsPerMinute);
    }

    protected virtual void MoveItems()
    {
        if (_items.Count == 0) return;

        for (int i = 0; i < _items.Count; i++)
        {
            BeltItem current = _items[i];
            float nextObstacleDist = _beltLength;

            // Item dahinter als Hindernis
            if (i > 0)
                nextObstacleDist = _items[i - 1].CurrentDistance - _itemSpacing;

            // Vorderstes Item: prüfen ob nächster Holder voll ist
            if (i == 0)
            {
                IItemHolder nextHolder = GetOutputHolder();
                if (nextHolder == null || nextHolder.IsFull)
                    nextObstacleDist = Mathf.Min(nextObstacleDist, _beltLength);
            }

            if (current.CurrentDistance < nextObstacleDist)
            {
                current.CurrentDistance += _speed * Time.deltaTime;
                current.CurrentDistance = Mathf.Min(current.CurrentDistance, nextObstacleDist);
            }

            // Visual-Position aktualisieren
            if (_startPoint != null && _endPoint != null && current.VisualObj != null)
            {
                float t = Mathf.Clamp01(current.CurrentDistance / _beltLength);
                current.VisualObj.transform.position = GetPositionOnPath(t);
            }
        }
    }

    protected virtual Vector3 GetPositionOnPath(float t)
    {
        return Vector3.Lerp(_startPoint.position, _endPoint.position, t);
    }

    private void CheckOutput()
    {
        if (_items.Count == 0) return;
        if (_items[0].CurrentDistance >= _beltLength)
            TryPushToNext(_items[0]);
    }

    private void TryPushToNext(BeltItem item)
    {
        if (_items.Count == 0 || _items[0] != item) return;

        IItemHolder nextHolder = GetOutputHolder();
        if (nextHolder == null || nextHolder.IsFull)
        {
            item.CurrentDistance = _beltLength;
            return;
        }

        if (nextHolder.TryAcceptItem(item.Definition, item.VisualObj))
            _items.RemoveAt(0);
        else
            item.CurrentDistance = _beltLength;
    }

    /// <summary>
    /// Holt den IItemHolder am Output-Port. Fallback auf transform.forward falls kein Port definiert.
    /// </summary>
    private IItemHolder GetOutputHolder()
    {
        if (_outputPort != null)
            return GetHolderAtPort(_outputPort);

        // Fallback: direkt vor dem Belt (für Belts ohne Definition)
        return _grid?.GetHolderAt(transform.position + transform.forward * GridStepSize);
    }

    // --- IItemHolder ---

    public override bool IsFull
    {
        get
        {
            if (_items.Count == 0) return false;
            return _items[_items.Count - 1].CurrentDistance < _itemSpacing;
        }
    }

    public override bool TryAcceptItem(ItemDefinition itemDef, GameObject existingVisual = null)
    {
        if (IsFull) return false;

        float startDistance = 0f;

        // Wenn ein Visual übergeben wird, Position auf dem Belt berechnen
        if (existingVisual != null && _startPoint != null && _endPoint != null)
        {
            Vector3 beltDir = (_endPoint.position - _startPoint.position).normalized;
            Vector3 toItem = existingVisual.transform.position - _startPoint.position;
            startDistance = Mathf.Clamp(Vector3.Dot(toItem, beltDir), 0f, _beltLength);
        }

        BeltItem newItem = new BeltItem
        {
            Definition = itemDef,
            CurrentDistance = startDistance
        };

        if (existingVisual != null)
        {
            newItem.VisualObj = existingVisual;
            PlaceVisualOnBelt(newItem.VisualObj, itemDef);
        }
        else if (itemDef.VisualPrefab != null && _startPoint != null)
        {
            newItem.VisualObj = Instantiate(itemDef.VisualPrefab, _startPoint.position, Quaternion.identity);
            PlaceVisualOnBelt(newItem.VisualObj, itemDef);
        }

        _items.Add(newItem);
        return true;
    }

    protected void PlaceVisualOnBelt(GameObject obj, ItemDefinition def)
    {
        Vector3 worldPos = obj.transform.position;

        obj.transform.SetParent(transform);
        obj.transform.localScale = def.VisualPrefab != null
            ? def.VisualPrefab.transform.localScale
            : Vector3.one;
        obj.transform.position = worldPos;
        obj.transform.rotation = Quaternion.identity;

        WorldItem wi = obj.GetComponent<WorldItem>() ?? obj.AddComponent<WorldItem>();
        wi.Initialize(def, this);

        if (!obj.GetComponent<Collider>())
        {
            var box = obj.AddComponent<BoxCollider>();
            box.size = Vector3.one * 0.4f;
            box.isTrigger = true;
        }
    }

    public override bool TryTakeItem(WorldItem worldItem)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].VisualObj == worldItem.gameObject)
            {
                _items.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    // --- Initialization ---

    /// <summary>
    /// Wird vom BeltPlacerTool aufgerufen um Start/End-Punkte programmatisch zu setzen.
    /// </summary>
    public void InitializeEndpoints(Vector3 startWorldPos, Vector3 endWorldPos)
    {
        if (_startPoint == null)
        {
            _startPoint = new GameObject("StartPoint").transform;
            _startPoint.SetParent(transform);
        }
        if (_endPoint == null)
        {
            _endPoint = new GameObject("EndPoint").transform;
            _endPoint.SetParent(transform);
        }

        _startPoint.position = startWorldPos;
        _endPoint.position = endWorldPos;

        CalculateSpeed();
        _initialized = true;
    }

    // --- IInteractable ---

    public string GetInteractPrompt()
    {
        if (_items.Count == 0) return "";
        return $"[E] {_items[0].Definition.Name} aufheben";
    }

    public void OnInteract(InventorySystem inventory)
    {
        if (_items.Count == 0) return;

        BeltItem front = _items[0];
        if (inventory.AddItem(front.Definition))
        {
            if (front.VisualObj != null) Destroy(front.VisualObj);
            _items.RemoveAt(0);
        }
        else
        {
            Debug.Log("Inventar voll!");
        }
    }
}