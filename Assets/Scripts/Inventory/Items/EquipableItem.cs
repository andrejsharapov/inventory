public class EquipableItem : InventoryItem
{
    public EquipableItem(Item item) : base(item) { }

    public override bool Use()
    {
        return true;
    }
}