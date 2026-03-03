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
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _itemSpacing = 0.6f;

    [Header("Visual Connections")]
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform _endPoint;

    private List<BeltItem> _items = new List<BeltItem>();

    // Override Start, aber ruf base.Start() auf!
    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        MoveItems();
        CheckOutput();
    }

    private void MoveItems()
    {
        if (_items.Count == 0) return;

        // Wir nutzen jetzt GridStepSize statt _length
        float totalLength = GridStepSize;

        for (int i = 0; i < _items.Count; i++)
        {
            BeltItem current = _items[i];
            float nextObstacleDist = totalLength;

            if (i > 0)
            {
                BeltItem itemAhead = _items[i - 1];
                nextObstacleDist = itemAhead.CurrentDistance - _itemSpacing;
            }

            if (current.CurrentDistance < nextObstacleDist)
            {
                current.CurrentDistance += _speed * Time.deltaTime;
                if (current.CurrentDistance > nextObstacleDist && i > 0)
                    current.CurrentDistance = nextObstacleDist;
            }

            // Visual Update
            float t = current.CurrentDistance / totalLength;
            if (_startPoint && _endPoint && current.VisualObj)
            {
                current.VisualObj.transform.position = Vector3.Lerp(_startPoint.position, _endPoint.position, t);
            }
        }
    }

    private void CheckOutput()
    {
        if (_items.Count == 0) return;

        BeltItem frontItem = _items[0];
        if (frontItem.CurrentDistance >= GridStepSize) // Check gegen GridStepSize
        {
            TryPushToNext(frontItem);
        }
    }

    private void TryPushToNext(BeltItem item)
    {
        if (_grid == null) return;

        Vector3 targetPos = transform.position + (transform.forward * GridStepSize);
        IItemHolder nextHolder = _grid.GetHolderAt(targetPos);

        if (nextHolder != null && nextHolder.TryAcceptItem(item.Definition, item.VisualObj))
        {
            _items.RemoveAt(0);
        }
        else
        {
            item.CurrentDistance = GridStepSize; // Stau
        }
    }

    // --- Interface Implementation ---

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

    public override bool TryAcceptItem(ItemDefinition itemDef, GameObject existingVisual = null)
    {
        if (_items.Count > 0)
        {
            BeltItem lastItem = _items[_items.Count - 1];
            if (lastItem.CurrentDistance < _itemSpacing) return false;
        }

        BeltItem newItem = new BeltItem
        {
            Definition = itemDef,
            CurrentDistance = 0f
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

    private void PlaceVisualOnBelt(GameObject obj, ItemDefinition def)
    {
        // Erst losloesen damit kein falscher Parent-Scale drinsteckt
        obj.transform.SetParent(null);
        Vector3 worldScale = obj.transform.localScale;
        obj.transform.SetParent(transform);

        // Scale in World-Space wiederherstellen
        obj.transform.localScale = new Vector3(
            worldScale.x / transform.lossyScale.x,
            worldScale.y / transform.lossyScale.y,
            worldScale.z / transform.lossyScale.z
        );

        obj.transform.position = _startPoint != null ? _startPoint.position : transform.position;
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

    // --- IInteractable ---

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
            // Visual zerst�ren
            if (frontItem.VisualObj != null)
                Destroy(frontItem.VisualObj);

            _items.RemoveAt(0);
            Debug.Log($"{frontItem.Definition.Name} aufgehoben!");
        }
        else
        {
            Debug.Log("Inventar voll!");
        }
    }
}