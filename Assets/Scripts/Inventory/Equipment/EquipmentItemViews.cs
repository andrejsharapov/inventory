using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentItemViews : MonoBehaviour
{
    [SerializeField] private bool _autoDisableAll = true;
    [SerializeField] private EquipmentItemView[] _equipmentItemViews;

    private Inventory _inventory;

#if UNITY_EDITOR
    private void OnValidate()
    {
        foreach (var itemView in _equipmentItemViews)
        {
            foreach (Transform child in itemView.ActiveParent)
                child.gameObject.SetActive(false);
            foreach (Transform child in itemView.InactiveParent)
                child.gameObject.SetActive(false);

            if (itemView.ActiveParent)
                itemView.ActiveParent.gameObject.SetActive(true);

            if (itemView.InactiveParent)
                itemView.InactiveParent.gameObject.SetActive(true);
        }
    }
#endif

    void Start()
    {
        _inventory = InventorySession.Instance.PlayerInventory;

        foreach (var itemView in _equipmentItemViews)
            itemView.Initialize();
            
        Subscribe();
        GetAndSetEquipment();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            SwitchActive(Item.EquipmentType.Weapon);
    }

    private void GetAndSetEquipment()
    {
        var slots = _inventory?.Equipment?.Slots;
        if (slots == null) return;

        foreach (var slot in slots)
            OnEquipmentChanged(slot.Key, slot.Value?.Item?.ItemConfig);
    }

    private void OnEquipmentChanged(Item.EquipmentType type, Item item)
    {
        SetEquiped(type,item, true);
    }

    private void Subscribe()
    {
        if (_inventory?.Equipment != null)
            _inventory.Equipment.EquipmentChanged += OnEquipmentChanged;
    }

    private void Unsubscribe()
    {
        if (_inventory?.Equipment != null)
            _inventory.Equipment.EquipmentChanged -= OnEquipmentChanged;
    }

    public void SetActive(Item.EquipmentType equipmentType, bool active)
    {
        var items = GetItemsByEquipmentType(equipmentType);

        foreach (var item in items)
        {
            if (item.IsEquiped)
            {
                item.SetActive(active);
            }
        }
    }
    public bool SwitchActive(Item.EquipmentType equipmentType)
    {
        var items = GetItemsByEquipmentType(equipmentType);

        foreach (var item in items)
        {
            if (item.IsEquiped)
            {
                item.SwitchActive();
                return item.IsActive;
            }
        }

        return false;
    }

    public void SetEquiped(Item.EquipmentType equipmentType, Item itemConfig, bool equiped)
    {
        var items = GetItemsByEquipmentType(equipmentType);

        foreach (var item in items)
        {
            item.SetEquiped(itemConfig && itemConfig == item.ItemConfig);
        }
    }

    private EquipmentItem[] GetItemsByEquipmentType(Item.EquipmentType equipmentType)
    {
        List<EquipmentItem> equipmentItems = new();

        foreach (var itemView in _equipmentItemViews)
        {
            var items = itemView.GetItemsByEquipmentType(equipmentType);

            foreach (var item in items)
                equipmentItems.Add(item);
        }

        return equipmentItems.ToArray();
    }


    [System.Serializable]
    public class EquipmentItemView
    {
        [SerializeField] private Item[] _itemConfigs;

        [Space]
        [SerializeField] private Transform _activeParent;
        [SerializeField] private Transform _inactiveParent;

        private Dictionary<Item, EquipmentItem> _equipmentItems;

        public readonly Dictionary<Item, EquipmentItem> EquipmentItems;
        public Transform ActiveParent => _activeParent;
        public Transform InactiveParent => _inactiveParent;

        public void Initialize()
        {
            CreateEquipments();

            if (_activeParent)
                _activeParent.gameObject.SetActive(true);

            if (_inactiveParent)
                _inactiveParent.gameObject.SetActive(true);

            foreach (var kvp in _equipmentItems)
                {
                    var key = kvp.Key;
                    var value = kvp.Value;

                    Debug.Log($"{key} => {value.ItemConfig.itemName} (active found: {value?.ActivePrefab != null} | inactive found: {value?.InactivePrefab != null})");
                }
        }

        private void CreateEquipments()
        {
            _equipmentItems = new();

            foreach (var itemConfig in _itemConfigs)
            {
                if (itemConfig == null || itemConfig.prefab == null) continue;

                string prefabName = itemConfig.prefab.name;

                GameObject activePrefab = null;
                GameObject inactivePrefab = null;

                foreach (Transform child in _inactiveParent)
                {
                    if (child.name == prefabName || child.name == prefabName + "(Clone)")
                    {
                        inactivePrefab = child.gameObject;
                        break;
                    }
                }

                if (inactivePrefab == null)
                    continue;

                foreach (Transform child in _activeParent)
                {
                    if (child.name == prefabName || child.name == prefabName + "(Clone)")
                    {
                        activePrefab = child.gameObject;
                        break;
                    }
                }

                var equipmentItem = new EquipmentItem(itemConfig, inactivePrefab, activePrefab);

                _equipmentItems[itemConfig] = equipmentItem;
            }
        }

        public bool TryGetItem(Item itemConfig, out EquipmentItem equipmentItem)
        {
            equipmentItem = null;

            if (itemConfig == null || _equipmentItems.ContainsKey(itemConfig) == false) return false;

            equipmentItem = _equipmentItems[itemConfig];

            return true;
        }

        public EquipmentItem[] GetItemsByEquipmentType(Item.EquipmentType equipmentType)
        {
            List<EquipmentItem> equipmentItems = new();

            foreach (var kvp in _equipmentItems)
            {
                var key = kvp.Key;
                var value = kvp.Value;

                if (key.equipmentType == equipmentType)
                    equipmentItems.Add(value);
            }

            return equipmentItems.ToArray();
        }
    }

    public class EquipmentItem
    {
        private GameObject _activePrefab;
        private GameObject _inactivePrefab;

        private Item _itemConfig;

        private bool _isEquiped;
        private bool _isActive;

        public GameObject ActivePrefab => _activePrefab;
        public GameObject InactivePrefab => _inactivePrefab;
        public Item ItemConfig => _itemConfig;
        public bool IsEquiped => _isEquiped;
        public bool IsActive => _isActive;


        public EquipmentItem(Item itemConfig, GameObject inactivePrefab, GameObject activePrefab = null)
        {
            _activePrefab = activePrefab;
            _inactivePrefab = inactivePrefab;
            _itemConfig = itemConfig;
        }

        public void SetEquiped(bool equiped)
        {
            if (_activePrefab)
                _activePrefab.SetActive(false);
            if (_inactivePrefab)
                _inactivePrefab.SetActive(equiped);

            _isActive = false;
            _isEquiped = equiped;
        }

        public void SetActive(bool active)
        {
            if (_activePrefab == false || _inactivePrefab == false) return;

            if (!_isEquiped) return;

            _activePrefab.SetActive(active);
            _inactivePrefab.SetActive(!active);

            _isActive = active;
            _isEquiped = true;
        }

        public void SwitchActive()
        {
            SetActive(!_isActive);
        }
    }
}
