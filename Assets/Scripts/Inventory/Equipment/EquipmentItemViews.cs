using System.Collections.Generic;
using UnityEngine;

public class EquipmentItemViews : MonoBehaviour
{
    [System.Serializable]
    public class EquipmentItemView
    {
        [SerializeField] private Item[] _itemConfigs;

        [Space]
        [SerializeField] private GameObject _itemObjectInactive;
        [SerializeField] private GameObject _itemObjectActive;

        public Item[] ItemConfigs => _itemConfigs;
        public GameObject ItemObjectInactive => _itemObjectInactive;
        public GameObject ItemObjectActive => _itemObjectActive;

        public bool IsEquipedActive { get; private set; } = false;
        public bool IsEquiped { get; private set; } = false;

        public bool SetEquipActive(bool equip)
        {
            var fromObject = equip ? _itemObjectInactive : _itemObjectActive;
            var toObject = equip ? _itemObjectActive : _itemObjectInactive;

            if (fromObject == null || toObject == null)
                return false;

            if (fromObject.activeInHierarchy)
            {
                toObject.SetActive(true);
                fromObject.SetActive(false);

                IsEquipedActive = equip;

                return true;
            }

            IsEquipedActive = false;

            return false;
        }

        public void SetActive(bool active)
        {
            if (active == false)
                IsEquipedActive = false;

            IsEquiped = active;

            if (_itemObjectActive)
                _itemObjectActive.SetActive(active && IsEquipedActive);

            if (_itemObjectInactive)
                _itemObjectInactive.SetActive(active && !IsEquipedActive);
        }

        public bool HasItemConfig(Item itemConfig)
        {
            foreach (var item in ItemConfigs)
                if (item == itemConfig)
                    return true;

            return false;
        }
    }

    [SerializeField] private bool _autoDisableAll = true;

    [SerializeField] private EquipmentItemView[] _equipmentItemViews;
    private Inventory _inventory;

    void OnValidate()
    {
        if (Application.isPlaying == false && _autoDisableAll)
            DisableAll();
    }


    void OnDestroy()
    {
        Unsubscribe();
    }

    void Awake()
    {
        DisableAll();
    }

    void Start()
    {
        _inventory = InventorySession.Instance.PlayerInventory;
        Subscribe();

        GetAndSetEquipment();
    }

    void GetAndSetEquipment()
    {
        var slots = _inventory?.Equipment?.Slots;

        if (slots == null) return;

        foreach (var slot in slots)
        {
            OnEquipmentChanged(slot.Key, slot.Value?.Item?.ItemConfig);
        }
    }
    void DisableAll()
    {
        foreach (var item in _equipmentItemViews)
            item.SetActive(false);
    }

    EquipmentItemView[] GetItemViewsByEquipmentType(Item.EquipmentType equipmentType)
    {
        List<EquipmentItemView> items = new();

        foreach (var itemView in _equipmentItemViews)
        {
            if (itemView == null) continue;
            foreach (var itemConfig in itemView.ItemConfigs)
            {
                if (itemConfig == null) continue;

                if (itemConfig.equipmentType == equipmentType)
                    items.Add(itemView);
            }
        }

        return items.ToArray();
    }

    public bool SwitchEquipActiveItem(Item.EquipmentType equipmentType)
    {
        var item = GetEquipedItemView(equipmentType);

        if (item != null)
            item.SetEquipActive(!item.IsEquipedActive);

        return item != null;
    }
    public bool SetEquipActiveItem(Item.EquipmentType equipmentType, bool active)
    {
        var item = GetEquipedItemView(equipmentType);

        if (item != null)
            item.SetEquipActive(active);

        return item != null;
    }

    private void Subscribe()
    {
        _inventory.Equipment.EquipmentChanged += OnEquipmentChanged;
    }
    private void Unsubscribe()
    {
        _inventory.Equipment.EquipmentChanged += OnEquipmentChanged;
    }

    public EquipmentItemView GetEquipedItemView(Item.EquipmentType equipmentType)
    {
        var items = GetItemViewsByEquipmentType(equipmentType);

        foreach (var item in items)
        {
            if (item.IsEquiped)
                return item;
        }

        return null;
    }

    private void OnEquipmentChanged(Item.EquipmentType equipmentType, Item itemConfig)
    {
        var items = GetItemViewsByEquipmentType(equipmentType);

        EquipmentItemView foundedWeapon = null;

        foreach (var item in items)
        {
            if (item == null) continue;
            
            item.SetActive(false);

            if (foundedWeapon == null && item.HasItemConfig(itemConfig))
                foundedWeapon = item;
        }

        if(foundedWeapon != null)
            foundedWeapon.SetActive(true);
    }
}
