using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

using static Item;

public class Inventory : IDisposable
{
    private readonly List<InventorySlot> _baseSlots = new();
    private readonly List<InventorySlot> _extraSlots = new();

    public Equipment Equipment { get; private set; }

    public IReadOnlyList<InventorySlot> Slots => _baseSlots.Concat(_extraSlots).ToList();

    public event Action InventoryChanged;



    private void OnEquipmentChanged(EquipmentType equipmentType, Item newItem)
    {
        InventoryChanged?.Invoke();
    }

    public Inventory(int slotsCount, InventorySession.StartItem[] startItems)
    {
        List<Item> items = new List<Item>();

        foreach (var startItem in startItems)
        {
            for (int i = 0; i < startItem.Quantity; i++)
            {
                items.Add(startItem.Item);
            }
        }

        Initialize(slotsCount, items.ToArray());
    }
    public Inventory(int slotsCount, Item[] startItems)
    {
        Initialize(slotsCount, startItems);
    }

    private void Initialize(int slotsCount, Item[] startItems)
    {
        Equipment = new(this);

        for (int i = 0; i < slotsCount; i++)
            _baseSlots.Add(new InventorySlot(this));

        foreach (Item item in startItems)
            AddItem(item);
        Equipment.EquipmentChanged += OnEquipmentChanged;

        InventoryChanged?.Invoke();
    }


    // public bool CanQuestBeCompleted(Quest quest)
    // {
    //     // Копируем текущие слоты (симулируем инвентарь)
    //     List<InventorySlot> simulatedInventory = Slots
    //         .Select(s =>
    //         {
    //             var copy = new InventorySlot(this, s.OwnerBag);
    //             copy.SetItem(s.Item, s.Quantity);
    //             return copy;
    //         })
    //         .ToList();

    //     int slotLimit = simulatedInventory.Count; // Кол-во слотов

    //     foreach (var reward in quest.itemRewards)
    //     {
    //         var item = reward.item;
    //         int quantity = reward.quantity;

    //         if (item == null)
    //             continue;

    //         int remaining = quantity;

    //         if (item.isStackable)
    //         {
    //             // Добавляем в существующие стеки
    //             for (int i = 0; i < simulatedInventory.Count; i++)
    //             {
    //                 var slot = simulatedInventory[i];

    //                 if (slot.Item != null && slot.Item.ItemConfig == item && slot.Quantity < item.maxStackSize)
    //                 {
    //                     int space = item.maxStackSize - slot.Quantity;
    //                     int toAdd = Mathf.Min(remaining, space);

    //                     slot.SetQuantity(slot.Quantity + toAdd);
    //                     remaining -= toAdd;

    //                     if (remaining == 0)
    //                         break;
    //                 }
    //             }

    //             // Добавляем в пустые слоты
    //             while (remaining > 0)
    //             {
    //                 int emptySlotIndex = simulatedInventory.FindIndex(s => s.Item == null);

    //                 if (emptySlotIndex == -1)
    //                 {
    //                     Debug.LogWarning("Недостаточно места в инвентаре для стакуемого предмета!");
    //                     return false;
    //                 }

    //                 int stackSize = Mathf.Min(item.maxStackSize, remaining);
    //                 simulatedInventory[emptySlotIndex].SetItem(InventoryItemFactory.Create(item), stackSize);
    //                 remaining -= stackSize;
    //             }
    //         }
    //         else
    //         {
    //             // Не стакуемые предметы — нужен отдельный слот на каждый

    //             int emptySlots = simulatedInventory.Count(s => s.Item == null);

    //             if (emptySlots < remaining)
    //             {
    //                 Debug.LogWarning("Недостаточно места в инвентаре для не стакуемых предметов!");
    //                 return false;
    //             }

    //             for (int i = 0; i < simulatedInventory.Count && remaining > 0; i++)
    //             {
    //                 if (simulatedInventory[i].Item == null)
    //                 {
    //                     simulatedInventory[i].SetItem(InventoryItemFactory.Create(item), 1);
    //                     remaining--;
    //                 }
    //             }
    //         }
    //     }

    //     return true;
    // }

    public void RecalculateExtraSlots()
    {
        var allSlots = Slots;
        var bagItems = allSlots
            .Where(s => s.Item is BagItem)
            .Select(s => s.Item)
            .ToList();

        var desiredBagData = new List<(InventoryItem item, int capacity)>();
        foreach (var bagItem in bagItems)
        {
            var bagConfig = bagItem.ItemConfig;
            desiredBagData.Add((bagItem, bagConfig.bagCapacity));
        }

        var groupedExtra = _extraSlots
            .GroupBy(s => s.OwnerBag)
            .ToDictionary(g => g.Key, g => g.ToList());

        var newExtraSlots = new List<InventorySlot>();
        foreach (var (bagItem, capacity) in desiredBagData)
        {
            groupedExtra.TryGetValue(bagItem, out var list);
            int exists = list?.Count ?? 0;
            for (int i = 0; i < Math.Min(exists, capacity); i++)
                newExtraSlots.Add(list[i]);
            for (int i = exists; i < capacity; i++)
                newExtraSlots.Add(new InventorySlot(this, bagItem));
        }

        var slotsToEvict = _extraSlots
            .Except(newExtraSlots)
            .Where(slot => slot.Item != null)
            .ToList();

        if (slotsToEvict.Count > 0)
        {
            var plan = new List<(InventorySlot from, InventorySlot to, int qty)>();
            bool allFit = true;

            var slotQtySnapshot = Slots.Except(slotsToEvict)
                .ToDictionary(s => s, s => s.Quantity);

            foreach (var fromSlot in slotsToEvict)
            {
                var item = fromSlot.Item;
                var config = item.ItemConfig;
                int left = fromSlot.Quantity;

                foreach (var slot in slotQtySnapshot.Keys
                            .Where(s => s.Item?.ItemConfig == config && config.isStackable && slotQtySnapshot[s] < config.maxStackSize))
                {
                    int canStack = config.maxStackSize - slotQtySnapshot[slot];
                    if (canStack <= 0) continue;
                    int moveQty = Math.Min(left, canStack);
                    if (moveQty > 0)
                    {
                        plan.Add((fromSlot, slot, moveQty));
                        slotQtySnapshot[slot] += moveQty;
                        left -= moveQty;
                    }
                    if (left <= 0) break;
                }

                while (left > 0)
                {
                    var emptySlot = slotQtySnapshot.Keys.FirstOrDefault(s => slotQtySnapshot[s] == 0 && s.Item == null);
                    if (emptySlot == null)
                    {
                        allFit = false;
                        break;
                    }

                    int chunk = config.isStackable ? Math.Min(left, config.maxStackSize) : 1;
                    plan.Add((fromSlot, emptySlot, chunk));
                    slotQtySnapshot[emptySlot] = chunk;
                    left -= chunk;
                }

                if (left > 0)
                {
                    allFit = false;
                    break;
                }
            }

            if (!allFit)
                return;

            foreach (var (from, to, qty) in plan)
            {
                if (to.Item == null)
                    to.SetItem(from.Item, qty);
                else
                    to.AddQuantity(qty);
                from.ReduceQuantity(qty);
                if (from.Quantity == 0)
                    from.Clear();
            }
        }

        _extraSlots.Clear();
        _extraSlots.AddRange(newExtraSlots);

        InventoryChanged?.Invoke();
    }
    public bool CanRemoveBag(InventoryItem bagItem)
    {
        var bagSlots = _extraSlots
            .Where(s => s.OwnerBag == bagItem)
            .ToList();

        foreach (var slot in bagSlots)
        {
            if (slot.Item != null || slot.Quantity > 0)
                return false;
        }

        return true;
    }

    public (bool added, InventorySlot slot) AddItem(InventorySlot inventorySlot) => AddItem(inventorySlot.Item, inventorySlot.Quantity);
    public (bool added, InventorySlot slot) AddItem(Item item, int quantity = 1) => AddItem(InventoryItemFactory.Create(item), quantity);
    public (bool Added, InventorySlot slot) AddItem(InventoryItem item, int quantity = 1)
    {
        InventorySlot firstSlot = null;
        while (quantity > 0)
        {
            var slot = Slots.FirstOrDefault(s =>
                s.Item != null &&
                s.Item.ItemConfig == item.ItemConfig &&
                s.Quantity < item.ItemConfig.maxStackSize);

            if (slot == null)
            {
                slot = Slots.FirstOrDefault(s => s.Item == null);
                if (slot == null) return (false, null);

                int toAdd = Mathf.Min(quantity, item.ItemConfig.maxStackSize);
                slot.SetItem(item, toAdd);
                quantity -= toAdd;
            }
            else
            {
                int canAdd = Mathf.Min(quantity, item.ItemConfig.maxStackSize - slot.Quantity);
                slot.AddQuantity(canAdd);
                quantity -= canAdd;
            }

            if (firstSlot == null)
                firstSlot = slot;
        }

        // QuestManager.Instance.ItemGathered(item, quantity);
        if (item is BagItem)
        {
            RecalculateExtraSlots();
        }

        InventoryChanged?.Invoke();
        return (true, firstSlot);
    }


    public bool RemoveItem(InventoryItem inventoryItem, int quantity = 1) => RemoveItem(inventoryItem.ItemConfig, quantity);

    public bool RemoveItem(Item itemConfig, int quantity = 1)
    {
        int total = Slots
            .Where(s => s.Item != null && s.Item.ItemConfig == itemConfig)
            .Sum(s => s.Quantity);

        if (total < quantity)
            return false;

        foreach (var slot in Slots
            .Where(s => s.Item != null && s.Item.ItemConfig == itemConfig && s.Quantity > 0)
            .OrderByDescending(s => s.Quantity))
        {
            if (slot.Item is BagItem && !CanRemoveBag(slot.Item)) continue;
            if (quantity <= 0)
                break;

            int toRemove = Mathf.Min(slot.Quantity, quantity);
            slot.ReduceQuantity(toRemove);
            quantity -= toRemove;
        }

        if (itemConfig.itemType == Item.ItemType.Bag)
        {
            RecalculateExtraSlots();
        }
        InventoryChanged?.Invoke();
        return true;
    }

    public void MoveOrSwapItems(InventorySlot from, InventorySlot to)
    {
        if (from == to) return;

        if (to.Item == null)
        {
            to.SetItem(from.Item, from.Quantity);
            from.Clear();
        }
        else if (to.Item.ItemConfig == from.Item.ItemConfig && to.Item.ItemConfig.isStackable)
        {
            int totalQuantity = to.Quantity + from.Quantity;
            int maxStack = to.Item.ItemConfig.maxStackSize;
            if (totalQuantity <= maxStack)
            {
                to.AddQuantity(from.Quantity);
                from.Clear();
            }
            else
            {
                int canAdd = maxStack - to.Quantity;
                to.AddQuantity(canAdd);
                from.ReduceQuantity(canAdd);
            }
        }
        else
        {
            var tempItem = to.Item;
            var tempQty = to.Quantity;

            to.SetItem(from.Item, from.Quantity);
            from.SetItem(tempItem, tempQty);
        }

        if (to is EquipmentSlot)
        {
            Equipment.OnEquipmentChanged(to.Item.ItemConfig.equipmentType, to.Item.ItemConfig);
        }
        else if (from is EquipmentSlot)
        {
            Equipment.OnEquipmentChanged(to.Item.ItemConfig.equipmentType, null);
        }
        else
            InventoryChanged?.Invoke();
    }

    public bool RemoveItemFromSlot(InventorySlot slot, int quantity = 1)
    {
        if (slot == null)
            throw new ArgumentNullException(nameof(slot));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        if (!Slots.Contains(slot) && !Equipment.Slots.Values.Contains(slot))
            throw new InvalidOperationException("Slot does not belong to this inventory.");

        var item = slot.Item;
        if (item is BagItem && !CanRemoveBag(item)) return false;

        if (item == null || slot.Quantity <= 0)
            return false;

        int toRemove = Mathf.Min(quantity, slot.Quantity);

        slot.ReduceQuantity(toRemove);

        if (slot.Quantity == 0)
            slot.Clear();

        if (item is BagItem)
        {
            RecalculateExtraSlots();
        }

        InventoryChanged?.Invoke();

        return true;
    }

    public bool SplitSlot(InventorySlot slot)
    {
        if (slot == null)
            throw new ArgumentNullException(nameof(slot));
        if (!Slots.Contains(slot))
            throw new InvalidOperationException("Slot does not belong to this inventory.");
        if (slot.Item == null || slot.Quantity <= 1)
            return false;

        InventorySlot freeSlot = Slots.FirstOrDefault(s => s.Item == null);
        if (freeSlot == null)
            return false;

        int oldQuantity = slot.Quantity;
        int half = oldQuantity / 2;
        int secondHalf = oldQuantity - half;

        slot.SetQuantity(half);

        InventoryItem newItem = InventoryItemFactory.Create(slot.Item.ItemConfig);

        freeSlot.SetItem(newItem, secondHalf);

        InventoryChanged?.Invoke();

        return true;
    }

    public bool HasItem(Item itemConfig, int quantity = 1)
    {
        int found = 0;
        foreach (var slot in Slots)
        {
            if (slot.Item.ItemConfig == itemConfig)
                found += slot.Quantity;

            if (found >= quantity)
                return true;
        }

        return false;
    }

    public bool HasSpace()
    {
        foreach (var slot in Slots)
        {
            if (slot.Item == null)
                return true;
        }

        return false;
    }
    public bool HasSpaceFor(Item item, int quantity = 1)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        int remaining = quantity;
        int maxStack = item.maxStackSize;
        bool isStackable = item.isStackable;

        foreach (var slot in Slots)
        {
            if (remaining <= 0)
                return true;

            if (slot.Item != null &&
                slot.Item.ItemConfig == item &&
                isStackable &&
                slot.Quantity < maxStack)
            {
                int canStack = maxStack - slot.Quantity;
                int used = Math.Min(remaining, canStack);
                remaining -= used;
            }
            else if (slot.Item == null)
            {
                int used = isStackable ? Math.Min(remaining, maxStack) : 1;
                remaining -= used;
            }
        }

        return remaining <= 0;
    }
    public void Dispose()
    {
        Equipment.EquipmentChanged -= OnEquipmentChanged;
    }
}
