
public class ScrapItem : InventoryItem
{
    public ScrapItem(Item item) : base(item) { }

    public override bool Use()
    {
        // GameBookUI.Instance.AddEntry(itemToRemove);

        return true;
    }
}