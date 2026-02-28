using UnityEngine;

public class OutputShaft : BaseGridMachine
{
    [Header("State")]
    public ItemDefinition CurrentItem { get; private set; }

    [Header("Visuals")]
    [SerializeField] private Transform _spawnPoint;

    private GameObject _currentVisualObj;

    // Start wird geerbt, aber wir rufen base auf
    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        if (CurrentItem != null) TryPushToNext();
    }

    private void TryPushToNext()
    {
        if (_grid == null) return;

        // Nutze GridStepSize aus der Basisklasse
        Vector3 targetPos = transform.position + (transform.forward * GridStepSize);
        IItemHolder nextHolder = _grid.GetHolderAt(targetPos);

        if (nextHolder != null && nextHolder.TryAcceptItem(CurrentItem, _currentVisualObj))
        {
            CurrentItem = null;
            _currentVisualObj = null;
        }
    }

    // Methoden vom Interface (aus BaseGridMachine)
    public override bool TryAcceptItem(ItemDefinition item, GameObject visualObj)
    {
        if (CurrentItem != null) return false;

        CurrentItem = item;

        if (visualObj != null)
        {
            _currentVisualObj = visualObj;
            _currentVisualObj.transform.SetParent(_spawnPoint);
            _currentVisualObj.transform.localPosition = Vector3.zero;
            _currentVisualObj.transform.localRotation = Quaternion.identity;
        }
        else
        {
            SpawnVisuals();
        }
        return true;
    }

    public override bool TryTakeItem(WorldItem itemScript)
    {
        if (_currentVisualObj != itemScript.gameObject) return false;
        CurrentItem = null;
        _currentVisualObj = null;
        return true;
    }

    // Spezifisch für MiningCore (kein Interface Zwang)
    public bool TryDeposit(ItemDefinition item, GameObject visualObj = null)
    {
        return TryAcceptItem(item, visualObj);
    }

    private void SpawnVisuals()
    {
        if (CurrentItem != null && CurrentItem.VisualPrefab != null)
        {
            _currentVisualObj = Instantiate(CurrentItem.VisualPrefab, _spawnPoint);
            _currentVisualObj.transform.localPosition = Vector3.zero;
            _currentVisualObj.transform.localRotation = Quaternion.identity;

            WorldItem worldItem = _currentVisualObj.AddComponent<WorldItem>();

            if (!_currentVisualObj.TryGetComponent<Collider>(out var col))
            {
                var box = _currentVisualObj.AddComponent<BoxCollider>();
                box.size = Vector3.one * 0.5f;
                col = box;
            }
            col.isTrigger = true;

            worldItem.Initialize(CurrentItem, this);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        float size = Application.isPlaying ? GridStepSize : 2f;
        Gizmos.DrawRay(transform.position, transform.forward * size);
        Gizmos.DrawWireSphere(transform.position + transform.forward * size, 0.2f);
    }
}