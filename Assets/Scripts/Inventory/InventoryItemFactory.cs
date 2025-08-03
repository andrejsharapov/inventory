using System;
using static Item;

public static class InventoryItemFactory
{
    public static InventoryItem Create(Item item)
    {
        switch (item.itemType)
        {
            case ItemType.Default:
                return new ResourceItem(item);

            case ItemType.Weapon:
                return new EquipableItem(item);

            case ItemType.Armor:
                return new EquipableItem(item);

            case ItemType.Bag:
                return new BagItem(item);

            case ItemType.Consumable:
                return new ResourceItem(item);

            case ItemType.Energy:
                return new EquipableItem(item);

            case ItemType.Healing:
                return new EquipableItem(item);

            case ItemType.Scrap:
                return new ResourceItem(item);

            case ItemType.Recipe:
                return new ResourceItem(item);

            case ItemType.Building:
                return new ResourceItem(item);

            default:
                throw new ArgumentOutOfRangeException($"Unknown item type: {item.itemType}");
        }
    }
}