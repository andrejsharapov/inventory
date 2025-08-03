using System;
using System.Collections.Generic;
using UnityEngine;
using static Item;

public class EquipmentView : MonoBehaviour
{
    [Serializable]
    private struct EquipmentSlotLink
    {
        public EquipmentType slotType;
        public InventorySlotView slotView;
    }

    [SerializeField] private EquipmentSlotLink[] _slots;


    private Dictionary<EquipmentType, InventorySlotView> _slotViews;
    private Equipment _equipment;

    public void Initialize(Equipment equipment)
    {
        _equipment = equipment;

        _slotViews = new Dictionary<EquipmentType, InventorySlotView>(_slots.Length);
        foreach (var slot in _slots)
        {
            if (slot.slotView != null)
                _slotViews[slot.slotType] = slot.slotView;
        }

        DrawSlots();
        _equipment.EquipmentChanged += DrawSlots;
    }

    private void OnDestroy()
    {
        if (_equipment != null)
            _equipment.EquipmentChanged -= DrawSlots;
    }

    private void DrawSlots()
    {
        foreach (var kvp in _slotViews)
        {
            EquipmentType slotType = kvp.Key;
            InventorySlotView slotView = kvp.Value;
            if (_equipment.Slots.TryGetValue(slotType, out var slot))
            {
                slotView.SetData(slot);
            }
            else
            {
                slotView.SetData(null);
            }
        }
    }
}