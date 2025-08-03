public abstract class InventoryItem
{
    public Item ItemConfig { get; }

    protected InventoryItem(Item item)
    {
        ItemConfig = item;
    }

    public abstract bool Use();
}
