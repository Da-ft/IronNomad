using UnityEngine;
using System.Collections.Generic;

public class ConveyorBelt : BaseGridMachine
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
                current.VisualObj.transform.localPosition = Vector3.Lerp(_startPoint.localPosition, _endPoint.localPosition, t);
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
            newItem.VisualObj.transform.SetParent(transform); // Umhängen

            if (_startPoint)
            {
                newItem.VisualObj.transform.position = _startPoint.position;
                newItem.VisualObj.transform.rotation = _startPoint.rotation;

                newItem.VisualObj.transform.localScale = Vector3.one;
            }
        }

        else if (itemDef.VisualPrefab != null && _startPoint != null)
        {
            newItem.VisualObj = Instantiate(itemDef.VisualPrefab, _startPoint.position, _startPoint.rotation);
            newItem.VisualObj.transform.SetParent(transform);

            SetupVisualItem(newItem.VisualObj, itemDef);
        }

        _items.Add(newItem);
        return true;
    }

    private void SetupVisualItem(GameObject obj, ItemDefinition def)
    {
        WorldItem wi = obj.GetComponent<WorldItem>();

        if (wi == null) wi = obj.AddComponent<WorldItem>();

        wi.Initialize(def, this);

        if(!obj.GetComponent<Collider>())
        {
            var box = obj.AddComponent<BoxCollider>();
            box.size = Vector3.one * 0.5f;
            box.isTrigger = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        // Wir können hier nicht auf GridStepSize zugreifen wenn das Spiel nicht läuft (außer wir holen es auch hier)
        // Einfachheitshalber nutzen wir lokal 2 oder holen es dynamisch
        float size = 2f;
        if (Application.isPlaying) size = GridStepSize;

        Vector3 end = transform.position + (transform.forward * size);
        Gizmos.DrawLine(transform.position, end);
        Gizmos.DrawWireSphere(end, 0.2f);
    }
}