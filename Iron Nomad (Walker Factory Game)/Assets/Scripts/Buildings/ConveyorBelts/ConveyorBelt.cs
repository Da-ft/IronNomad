using UnityEngine;
using System.Collections.Generic;

public class ConveyorBelt : MonoBehaviour, IItemHolder
{
    [System.Serializable]
    public class BeltItem
    {
        public ItemDefinition Definition;
        public GameObject VisualObj;
        public float CurrentDistance; // 0 = Start, Length = Ende
    }

    [Header("Settings")]
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _length = 2f; // Muss zur GridSize passen (meist 2)
    [SerializeField] private float _itemSpacing = 0.6f;

    [Header("Visual Connections")]
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform _endPoint;

    private List<BeltItem> _items = new List<BeltItem>();
    private WalkerGrid _grid;

    private void Start()
    {
        // Wir holen uns das Grid vom Walker (Parent)
        _grid = GetComponentInParent<WalkerGrid>();
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
            float nextObstacleDist = _length;

            if (i > 0)
            {
                BeltItem itemAhead = _items[i - 1];
                nextObstacleDist = itemAhead.CurrentDistance - _itemSpacing;
            }

            if (current.CurrentDistance < nextObstacleDist)
            {
                current.CurrentDistance += _speed * Time.deltaTime;
                if (current.CurrentDistance > nextObstacleDist && i > 0)
                {
                    current.CurrentDistance = nextObstacleDist;
                }
            }

            // Visual Update (Lerp)
            float t = current.CurrentDistance / _length;
            if (_startPoint && _endPoint && current.VisualObj)
            {
                current.VisualObj.transform.localPosition = Vector3.Lerp(
                    _startPoint.localPosition,
                    _endPoint.localPosition,
                    t
                );
            }
        }
    }

    private void CheckOutput()
    {
        if (_items.Count == 0) return;

        BeltItem frontItem = _items[0];

        // Ist das Item am Ende (oder darüber hinaus)?
        if (frontItem.CurrentDistance >= _length)
        {
            TryPushToNext(frontItem);
        }
    }

    private void TryPushToNext(BeltItem item)
    {
        if (_grid == null) return;

        // Ziel berechnen
        Vector3 targetPos = transform.position + (transform.forward * _length);

        // Nachbar suchen
        IItemHolder nextHolder = _grid.GetHolderAt(targetPos);

        if (nextHolder != null)
        {
            // Debug Log: Wer übergibt an wen?
             Debug.Log($"BAND {gameObject.name}: Versuche Übergabe an {((MonoBehaviour)nextHolder).name}...");

            if (nextHolder.TryAcceptItem(item.Definition))
            {
                Debug.Log("... ERFOLG! Item übergeben.");
                Destroy(item.VisualObj);
                _items.RemoveAt(0);
            }
            else
            {
                Debug.Log("... ABGELEHNT! Nachbar ist voll oder blockiert.");
                item.CurrentDistance = _length; // Stau
            }
        }
        else
        {
            Debug.Log($"BAND {gameObject.name}: Kein Nachbar an Position {targetPos} gefunden!");
            item.CurrentDistance = _length; // Item fällt runter / bleibt liegen
        }
    }

    //private void TryPushToNext(BeltItem item)
    //{
    //    if (_grid == null) return;

    //    // --- GRID LOGIK ---
    //    // Wohin schieben wir? Genau eine Band-Länge nach vorne (lokal Z)
    //    // Das entspricht genau der Koordinate des nächsten Feldes.
    //    Vector3 targetPos = transform.position + (transform.forward * _length);

    //    IItemHolder nextHolder = _grid.GetHolderAt(targetPos);

    //    if (nextHolder != null)
    //    {
    //        // Übergabe versuchen
    //        if (nextHolder.TryAcceptItem(item.Definition))
    //        {
    //            // Erfolg!
    //            Destroy(item.VisualObj);
    //            _items.RemoveAt(0);
    //        }
    //        else
    //        {
    //            // Nachbar ist voll -> Item bleibt am Ende liegen (Band staut sich)
    //            item.CurrentDistance = _length;
    //        }
    //    }
    //    else
    //    {
    //        // Kein Nachbar da -> Item bleibt liegen
    //        item.CurrentDistance = _length;
    //    }
    //}

    // --- Interface Implementation ---

    public bool TryTakeItem(WorldItem worldItem)
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

    public bool TryAcceptItem(ItemDefinition itemDef)
    {
        // Platz am Eingang prüfen
        if (_items.Count > 0)
        {
            BeltItem lastItem = _items[_items.Count - 1];
            if (lastItem.CurrentDistance < _itemSpacing) return false;
        }

        BeltItem newItem = new BeltItem();
        newItem.Definition = itemDef;
        newItem.CurrentDistance = 0f;

        if (itemDef.VisualPrefab != null && _startPoint != null)
        {
            newItem.VisualObj = Instantiate(itemDef.VisualPrefab, _startPoint.position, _startPoint.rotation);
            newItem.VisualObj.transform.SetParent(transform);

            // Components hinzufügen
            WorldItem wi = newItem.VisualObj.AddComponent<WorldItem>();
            wi.Initialize(itemDef, this);

            if (!newItem.VisualObj.GetComponent<Collider>())
            {
                var box = newItem.VisualObj.AddComponent<BoxCollider>();
                box.size = Vector3.one * 0.5f;
            }
        }

        _items.Add(newItem);
        return true;
    }

    // --- DEBUG GIZMOS ---
    private void OnDrawGizmos()
    {
        // Zeichnet einen grünen Pfeil vom Ende des Bandes zum Ziel
        Gizmos.color = Color.green;
        Vector3 end = transform.position + (transform.forward * _length);

        Gizmos.DrawLine(transform.position, end);
        Gizmos.DrawWireSphere(end, 0.2f);
    }
}