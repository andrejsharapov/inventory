public class ConsumableItem : InventoryItem
{
    public ConsumableItem(Item item) : base(item) { }

    public override bool Use()
    {
        return true;
    }
}