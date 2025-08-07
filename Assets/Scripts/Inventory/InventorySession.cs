using UnityEngine;

public class InventorySession : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryView _inventoryView;
    
    [Header("Parameters")]
    [SerializeField] private StartItem[] _startItems;
    [SerializeField] private int _slotsCount = 10;

    public Inventory PlayerInventory { get; private set; }
    public static InventorySession Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        PlayerInventory = new Inventory(_slotsCount, _startItems);

        if (_inventoryView)
            _inventoryView.Initialize(PlayerInventory);
    }
    private void OnDestroy()
    {
        PlayerInventory.Dispose();
    }

    [System.Serializable]
    public class StartItem
    {
        [SerializeField] private Item _item;
        [SerializeField] private int _quantity = 1;

        public Item Item => _item;
        public int Quantity => _quantity;

        public StartItem()
        {
            _quantity = 1;
        }
    }
}