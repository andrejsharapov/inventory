using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Item;

public class Equipment
{
    private readonly Dictionary<EquipmentType, EquipmentSlot> _slots;

    public IReadOnlyDictionary<EquipmentType, EquipmentSlot> Slots => _slots;

    public event Action EquipmentChanged;

    public Equipment(Inventory inventory)
    {
        _slots = Enum.GetValues(typeof(EquipmentType))
            .Cast<EquipmentType>()
            .ToDictionary(type => type, type => new EquipmentSlot(inventory, type));
    }

    public bool Unequip(EquipmentType slotType) => Unequip(_slots[slotType]);
    public bool Unequip(EquipmentSlot slot)
    {
        if (slot.Item == null)
            return false;

        slot.Clear();
        EquipmentChanged?.Invoke();
        return true;
    }


    public EquipmentSlot GetEquipmentSlot(EquipmentType equipmentType)
    {
        return _slots[equipmentType];
    }
}