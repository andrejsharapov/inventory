
public class RecipeItem : InventoryItem
{
    public RecipeItem(Item item) : base(item) { }

    public override bool Use()
    {
        // CraftingUI.Instance.LearnRecipe(itemToRemove);

        return true;
    }
}