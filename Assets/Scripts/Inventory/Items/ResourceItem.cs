public class ResourceItem : InventoryItem
{
    public ResourceItem(Item item) : base(item) { }

    public override bool Use()
    {
        return true;
    }
}
