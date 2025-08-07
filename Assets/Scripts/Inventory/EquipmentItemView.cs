using System;
using UnityEngine;

public class EquipmentItemView : MonoBehaviour
{
    [SerializeField] private Item _itemConfig;
    [SerializeField] private GameObject _itemObject;

    private Inventory _inventory;

    void Awake()
    {
        if (_itemConfig == null)
            enabled = false;

    }

    void OnDestroy()
    {
        Unsubscribe();
    }

    void Start()
    {
        _inventory = InventorySession.Instance.PlayerInventory;
        Subscribe();

        SetEquipment();
    }

    void SetEquipment()
    {
        var slots = _inventory?.Equipment?.Slots;

        if (slots == null) return;

        var equipItem = slots[_itemConfig.equipmentType];

        OnEquipmentChanged(_itemConfig.equipmentType,equipItem?.Item?.ItemConfig);
    }

    private void Subscribe()
    {
        _inventory.Equipment.EquipmentChanged += OnEquipmentChanged;
    }
    private void Unsubscribe()
    {
        _inventory.Equipment.EquipmentChanged += OnEquipmentChanged;
    }

    private void OnEquipmentChanged(Item.EquipmentType type, Item item)
    {
        if (type != _itemConfig?.equipmentType) return;

        _itemObject.SetActive(item == _itemConfig);
    }

}
