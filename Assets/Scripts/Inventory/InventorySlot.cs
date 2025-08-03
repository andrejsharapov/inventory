using System;
using UnityEngine;

public class InventorySlot
{
    public InventoryItem Item { get; private set; }
    public int Quantity { get; private set; }

    public Inventory Inventory { get; private set; }
    public InventoryItem OwnerBag { get; private set; }

    public InventorySlot(Inventory inventory, InventoryItem ownerBag = null)
    {
        Inventory = inventory;
        OwnerBag = ownerBag;
        
        SetItem(null,0);
    }

    public InventorySlot(Inventory inventory, InventoryItem inventoryItem, int slotID, int quantity = 1)
    {
        Inventory = inventory;

        if (quantity <= 0)
        {
            Debug.LogError("Item quantity can't be less than 1");
            quantity = 1;
        }

        SetItem(inventoryItem,quantity);
    }

    public void SetItem(InventoryItem inventoryItem, int quantity)
    {
        Item = inventoryItem;
        Quantity = quantity;
    }

    public void AddQuantity(int quantity)
    {
        Quantity += quantity;
    }

    public void ReduceQuantity(int amount)
    {
        Quantity -= amount;
        if (Quantity <= 0)
            Clear();
    }

    public void Clear()
    {
        Quantity = 0;
        Item = null;
    }

    public void SetQuantity(int quantity)
    {
        Quantity = quantity;
    }
}
