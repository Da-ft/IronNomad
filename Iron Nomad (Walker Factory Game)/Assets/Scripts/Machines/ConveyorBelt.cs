using UnityEngine;
using System.Collections.Generic;

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
    [SerializeField] private float _itemsPerMinute = 60f;
    [SerializeField] private float _itemSpacing = 0.6f;

    [Header("Visual Connections")]
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform _endPoint;

    private List<BeltItem> _items = new List<BeltItem>();
    private float _beltLength;
    private float _speed;

    protected override void Start()
    {
        base.Start();
        CalculateSpeed();
    }

    private void CalculateSpeed()
    {
        _beltLength = Vector3.Distance(_startPoint.position, _endPoint.position);
        _speed = _beltLength / (60f / _itemsPerMinute);
    }

    private void Update()
    {
        MoveItems();
        CheckOutput();
    }

    private void MoveItems()
    {
        if (_items.Count == 0) return;

        for (int i = 0; i < _items.Count; i++)
        {
            BeltItem current = _items[i];
            float nextObstacleDist = _beltLength;

            if (i > 0)
            {
                BeltItem itemAhead = _items[i - 1];
                nextObstacleDist = itemAhead.CurrentDistance - _itemSpacing;
            }

            // Vorderstes Item — Nachfolger prüfen
            if (i == 0)
            {
                ConstructibleBuilding constructible = GetComponent<ConstructibleBuilding>();
                IItemHolder nextHolder = constructible?.Definition != null
                    ? GetNextHolder(constructible.Definition.OutputDirections, constructible.Rotation)
                    : _grid?.GetHolderAt(transform.position + transform.forward * GridStepSize);

                if (nextHolder != null && nextHolder.IsFull)
                    nextObstacleDist = Mathf.Min(nextObstacleDist, _beltLength);
                else if (nextHolder == null)
                    nextObstacleDist = Mathf.Min(nextObstacleDist, _beltLength);
            }

            if (current.CurrentDistance < nextObstacleDist)
            {
                current.CurrentDistance += _speed * Time.deltaTime;
                current.CurrentDistance = Mathf.Min(current.CurrentDistance, nextObstacleDist);
            }

            // Position im Worldspace
            if (_startPoint && _endPoint && current.VisualObj)
            {
                float t = Mathf.Clamp01(current.CurrentDistance / _beltLength);
                current.VisualObj.transform.position = Vector3.Lerp(
                    _startPoint.position,
                    _endPoint.position,
                    t
                );
            }
        }
    }

    private void CheckOutput()
    {
        if (_items.Count == 0) return;

        BeltItem frontItem = _items[0];
        if (frontItem.CurrentDistance >= _beltLength)
            TryPushToNext(frontItem);
    }

    private void TryPushToNext(BeltItem item)
    {
        if (_items.Count == 0 || _items[0] != item) return;

        ConstructibleBuilding constructible = GetComponent<ConstructibleBuilding>();
        IItemHolder nextHolder = constructible?.Definition != null
            ? GetNextHolder(constructible.Definition.OutputDirections, constructible.Rotation)
            : _grid.GetHolderAt(transform.position + transform.forward * GridStepSize);

        if (nextHolder == null || nextHolder.IsFull)
        {
            item.CurrentDistance = _beltLength;
            return;
        }
        Debug.Log($"Item Weltpos: {item.VisualObj.transform.position}");
        Debug.Log($"Alter EndPoint: {_endPoint.position}");
        if (nextHolder.TryAcceptItem(item.Definition, item.VisualObj))
            _items.RemoveAt(0);
        else
            item.CurrentDistance = _beltLength;

        Debug.Log($"Versuche Übergabe: Items im Belt vorher: {_items.Count}");
        if (nextHolder.TryAcceptItem(item.Definition, item.VisualObj))
        {
            Debug.Log($"Übergabe erfolgreich, Items danach: {_items.Count - 1}");
            _items.RemoveAt(0);
        }
    }

    public override bool IsFull
    {
        get
        {
            if (_items.Count == 0) return false;
            BeltItem last = _items[_items.Count - 1];
            return last.CurrentDistance < _itemSpacing;
        }
    }

    public override bool TryAcceptItem(ItemDefinition itemDef, GameObject existingVisual = null)
    {
        Debug.Log($"TryAcceptItem aufgerufen, existingVisual null: {existingVisual == null}");

        if (IsFull) return false;

        float startDistance = 0f;
        if (existingVisual != null && _startPoint != null && _endPoint != null)
        {
            // Weltposition vor PlaceVisualOnBelt speichern
            Vector3 worldPos = existingVisual.transform.position;
            Vector3 beltDir = (_endPoint.position - _startPoint.position).normalized;
            Vector3 toItem = worldPos - _startPoint.position;
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
        Debug.Log($"Neuer StartPoint: {_startPoint.position}");
        Debug.Log($"Berechnete startDistance: {startDistance}");
        _items.Add(newItem);
        return true;
    }

    private void PlaceVisualOnBelt(GameObject obj, ItemDefinition def)
    {
        // Weltposition vor Parent-Wechsel merken
        Vector3 worldPos = obj.transform.position;

        obj.transform.SetParent(transform);
        obj.transform.localScale = def.VisualPrefab != null
            ? def.VisualPrefab.transform.localScale
            : Vector3.one;

        // Weltposition wiederherstellen — kein Sprung
        obj.transform.position = worldPos;
        obj.transform.rotation = Quaternion.identity;

        WorldItem wi = obj.GetComponent<WorldItem>();
        if (wi == null) wi = obj.AddComponent<WorldItem>();
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

    public string GetInteractPrompt()
    {
        if (_items.Count == 0) return "";
        return $"[E] {_items[0].Definition.Name} aufheben";
    }

    public void OnInteract(InventorySystem inventory)
    {
        if (_items.Count == 0) return;

        BeltItem frontItem = _items[0];
        if (inventory.AddItem(frontItem.Definition))
        {
            if (frontItem.VisualObj != null)
                Destroy(frontItem.VisualObj);
            _items.RemoveAt(0);
        }
        else
        {
            Debug.Log("Inventar voll!");
        }
    }
}