public class BagItem : InventoryItem
{
    public BagItem(Item item) : base(item) { }

    public override bool Use()
    {
        return true;
    }
}