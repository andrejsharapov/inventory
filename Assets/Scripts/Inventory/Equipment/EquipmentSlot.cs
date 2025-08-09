using static Item;

public class EquipmentSlot : InventorySlot
{
    public EquipmentType SlotType { get; }
    public EquipmentSlot(Inventory inventory, EquipmentType slotType) : base(inventory)
    {
        SlotType = slotType;
    }
}