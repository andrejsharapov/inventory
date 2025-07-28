using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
  public Item item;
  public int count;

  public InventorySlot(Item item, int count = 1)
  {
    this.item = item;
    this.count = count;
  }

  public InventorySlot()
  {
    item = null;
    count = 0;
  }

  public void Clear()
  {
    item = null;
    count = 0;
  }
}

public class Inventory : MonoBehaviour
{
  public static Inventory instance;
  public List<InventorySlot> items = new List<InventorySlot>();
  [SerializeField]
  private int slotLimit = 20; // Максимальное количество слотов в инвентаре
  public int SlotLimit { get; private set; }

  public int maxBagItems = 6; // Максимальное количество предметов типа Bag

  public delegate void OnInventoryChanged();
  public OnInventoryChanged onInventoryChanged;

  [Header("!Do not change")]
  public int ItemAdded = 0;

  void Awake()
  {
    if (instance == null)
    {
      instance = this;
      DontDestroyOnLoad(gameObject); // Важно для сохранения между сценами
      Debug.Log("Inventory Awake - Instance ID: " + GetInstanceID());
    }
    else
    {
      if (instance != this) // Добавляем проверку, является ли текущий экземпляр тем же самым
      {
        Debug.LogWarning("Обнаружен еще один инвентарь! Уничтожение: " + gameObject.name + GetInstanceID());
        Destroy(gameObject);
        return; // Не выполнять остальной код Awake() для уничтоженного объекта
      }
    }

    SlotLimit = slotLimit;
  }

  // Добавляет предмет в инвентарь
  public void Add(Item item)
  {
    Add(item, 1);
  }

  // Добавляет предмет и количество в инвентарь
  public void Add(Item item, int count)
  {
    if (item == null)
    {
      Debug.LogWarning("Attempted to add null item!");
      return;
    }

    if (count <= 0)
    {
      Debug.LogWarning("Attempted to add zero or negative items!");
      return;
    }

    Debug.Log($"Attempting to add {count} {item.itemName} to inventory.");

    // Если предмет не стекуемый или нечего добавлять, сразу добавляем в пустой слот.
    if (!item.isStackable || count <= 0)
    {
      AddNonStackableItem(item, count);
      return;
    }

    // Поиск существующего стака
    InventorySlot existingSlot = FindExistingStack(item);

    if (existingSlot != null)
    {
      // Стек найден, добавляем
      int spaceAvailable = item.maxStackSize - existingSlot.count; // Считаем место в стаке
      if (spaceAvailable >= count)
      {
        // Добавляем полностью в стак
        existingSlot.count += count;

        // Отправляем сообщение о собранном предмете и количестве
        // QuestManager.Instance.ItemGathered(item, count);
        Debug.Log($"Добавлено {count} {item.itemName} в существующий стак. Total: {existingSlot.count}");
      }
      else
      {
        // Не всё помещается в существующий стак
        existingSlot.count = item.maxStackSize;

        // Отправляем сообщение о собранном предмете и количестве
        // QuestManager.Instance.ItemGathered(item, spaceAvailable);
        Debug.Log($"Добавлено {spaceAvailable} {item.itemName} в существующий стак (переполнение). Total: {existingSlot.count}");

        AddNonStackableItem(item, count - spaceAvailable); // Добавляем остаток как отдельные предметы
      }

      onInventoryChanged?.Invoke(); // Вызываем событие
      return;
    }

    // Стак не найден, добавляем в пустой слот.
    AddNonStackableItem(item, count);
  }

  // Метод для подсчета предметов определенного типа в инвентаре
  public int CountItemsOfType(Item.ItemType type) // Используем Item.ItemType
  {
    int count = 0;

    foreach (var slot in items)
    {
      if (slot != null && slot.item != null && slot.item.itemType == type)
      {
        count++; // Просто инкрементируем, т.к. мы считаем *количество слотов* с предметами нужного типа
      }
    }

    return count;
  }

  // Добавление не стакуемых предметов или предметов, которые не поместились в стак
  private void AddNonStackableItem(Item item, int count)
  {
    Debug.Log($"Adding {count} non-stackable {item.itemName}.");

    // Сначала попробуем создать новые стеки, если это возможно
    while (count > 0 && item.isStackable)
    {
      // Ищем пустой слот
      int emptySlotIndex = FindFirstEmptySlot();

      // Если пустых слотов нет, но есть место в инвентаре, добавляем в конец
      if (emptySlotIndex == -1 && items.Count < SlotLimit)
      {
        emptySlotIndex = items.Count;
        items.Add(new InventorySlot());
      }

      // Если нашли место, создаем новый стак
      if (emptySlotIndex != -1)
      {
        // Определяем, сколько можно поместить в этот стак
        int stackSize = Mathf.Min(item.maxStackSize, count);
        // Создаем новый стак
        ItemAdded = 1;
        items[emptySlotIndex] = new InventorySlot(item, stackSize);
        // Обновляем счетчик и удаляем предметы
        count -= stackSize;
        //Сообщаем о собранном предмете
        // QuestManager.Instance.ItemGathered(item, stackSize);
        Debug.Log($"Добавлено {stackSize} {item.itemName} в новый стак инвентаря.");
        onInventoryChanged?.Invoke(); // Вызываем событие
      }
      else
      {
        // Если нет места, выходим из цикла
        break;
      }
    }

    // Если остались предметы, которые не влезли в стеки, добавляем по одному
    for (int i = 0; i < count; i++)
    {
      if (item.itemType == Item.ItemType.Bag)
      {
        SetSlotLimit(SlotLimit + item.bagCapacity);
      }

      // Ищем первый пустой слот
      int emptySlotIndex = FindFirstEmptySlot();

      if (emptySlotIndex != -1)
      {
        ItemAdded = 1;
        items[emptySlotIndex] = new InventorySlot(item, 1);
        //Сообщаем о собранном предмете
        // QuestManager.Instance.ItemGathered(item, 1);
        Debug.Log($"Added {item.itemName} to inventory.");
      }
      else if (items.Count < SlotLimit)
      {
        ItemAdded = 1;
        // Пустых слотов нет, но есть место в инвентаре, добавляем в конец
        items.Add(new InventorySlot(item, 1));
        //Сообщаем о собранном предмете
        // QuestManager.Instance.ItemGathered(item, 1);
        Debug.Log($"Added {item.itemName} to inventory.");
      }
      else
      {
        Debug.Log("Инвентарь полон!");
        ItemAdded = 0;
        return; // Если инвентарь заполнен, выходим
      }
      onInventoryChanged?.Invoke();
    }

    onInventoryChanged?.Invoke(); // Вызываем событие
  }

  // Поиск существующего стака
  private InventorySlot FindExistingStack(Item item)
  {
    foreach (var slot in items)
    {
      if (slot.item != null && slot.item == item && slot.item.isStackable && slot.count < item.maxStackSize)
      {
        return slot;
      }
    }
    return null;
  }

  // Проверяет наличие предмета в нужном количестве
  public bool HasItem(Item item, int count)
  {
    int itemCount = 0;

    foreach (InventorySlot slot in items)
    {
      if (slot.item == item)
      {
        itemCount += slot.count;
      }
    }
    return itemCount >= count;
  }

  // Удаляет предмет из инвентаря
  public void Remove(int slotIndex)
  {
    if (slotIndex < 0 || slotIndex >= items.Count)
    {
      Debug.LogError("Некорректный индекс слота: " + slotIndex);
      return;
    }

    InventorySlot slotToRemove = items[slotIndex];
    if (slotToRemove.item == null) return;

    // Если это сумка, меняем SlotLimit
    if (slotToRemove.item.itemType == Item.ItemType.Bag)
    {
      // Проверяем, достаточно ли свободных слотов для удаления сумки
      int freeSlots = SlotLimit - items.Count;
      if (freeSlots < slotToRemove.item.bagCapacity)
      {
        Debug.Log("Нельзя удалить сумку: недостаточно места в инвентаре.");
        return; // Отмена удаления
      }
      SetSlotLimit(SlotLimit - slotToRemove.item.bagCapacity); // Уменьшаем количество слотов
    }

    slotToRemove.item = null; // Удаляем ссылку на предмет
    slotToRemove.count = 0; // Сбрасываем количество

    onInventoryChanged?.Invoke();
  }

  // Обновляет информацию о слоте
  public void UpdateSlot(InventorySlot slot, Item item)
  {
    onInventoryChanged?.Invoke();
  }

  // Публичный метод для изменения лимита слотов
  public void SetSlotLimit(int newLimit)
  {
    if (newLimit > 0)
    {
      SlotLimit = newLimit;
      onInventoryChanged?.Invoke(); // Уведомляем об изменении
    }
    else
    {
      Debug.LogError("Лимит слотов должен быть больше 0.");
    }
  }

  // Ищем первый пустой слот в инвентаре
  private int FindFirstEmptySlot()
  {
    for (int i = 0; i < items.Count; i++)
    {
      if (items[i].item == null)
      {
        return i;
      }
    }
    return -1; // Нет пустых слотов
  }

  // public bool CanQuestBeCompleted(Quest quest)
  // {
  //   // Создаем копию инвентаря для симуляции добавления наград
  //   List<InventorySlot> simulatedInventory = new List<InventorySlot>();
  //   foreach (var slot in items)
  //   {
  //     simulatedInventory.Add(new InventorySlot(slot.item, slot.count));
  //   }

  //   int simulatedSlotLimit = SlotLimit;

  //   // Проверяем награды квеста
  //   foreach (var reward in quest.itemRewards)
  //   {
  //     Item item = reward.item;
  //     int quantity = reward.quantity;

  //     if (item == null) continue; // Пропускаем null награды

  //     int remaining = quantity; // Сколько предметов еще нужно добавить

  //     if (item.isStackable)
  //     {
  //       // Сначала пытаемся добавить в существующие стаки
  //       for (int i = 0; i < simulatedInventory.Count; i++)
  //       {
  //         InventorySlot slot = simulatedInventory[i];
  //         if (slot.item == item && slot.count < item.maxStackSize)
  //         {
  //           int spaceAvailable = item.maxStackSize - slot.count;
  //           int addAmount = Mathf.Min(remaining, spaceAvailable); // Сколько можем добавить в этот стак
  //           slot.count += addAmount;
  //           remaining -= addAmount;

  //           simulatedInventory[i] = slot; // Обновляем слот
  //         }
  //       }

  //       // Если остались предметы, создаем новые стаки
  //       while (remaining > 0)
  //       {
  //         // Ищем пустой слот
  //         int emptySlotIndex = -1;
  //         for (int i = 0; i < simulatedInventory.Count; i++)
  //         {
  //           if (simulatedInventory[i].item == null)
  //           {
  //             emptySlotIndex = i;
  //             break;
  //           }
  //         }

  //         // Если пустых слотов нет, но есть место в инвентаре, добавляем в конец
  //         if (emptySlotIndex == -1 && simulatedInventory.Count < simulatedSlotLimit)
  //         {
  //           emptySlotIndex = simulatedInventory.Count;
  //           simulatedInventory.Add(new InventorySlot());
  //         }

  //         // Если нашли место, создаем новый стак
  //         if (emptySlotIndex != -1)
  //         {
  //           int stackSize = Mathf.Min(item.maxStackSize, remaining);
  //           simulatedInventory[emptySlotIndex] = new InventorySlot(item, stackSize);
  //           remaining -= stackSize;
  //         }
  //         else
  //         {
  //           // Если нет места, значит не можем сдать квест
  //           Debug.LogWarning("Недостаточно места в инвентаре для получения стакуемого предмета!");
  //           return false;
  //         }
  //       }
  //     }
  //     else
  //     {
  //       // Если предмет не стакуемый, проверяем, хватит ли места для всех экземпляров
  //       int emptySlots = 0;
  //       foreach (var slot in simulatedInventory)
  //       {
  //         if (slot.item == null)
  //         {
  //           emptySlots++;
  //         }
  //       }

  //       if (simulatedInventory.Count < simulatedSlotLimit)
  //       {
  //         emptySlots++;
  //       }

  //       if (emptySlots < quantity)
  //       {
  //         Debug.LogWarning("Недостаточно места в инвентаре для получения не стакуемых предметов!");
  //         return false;
  //       }
  //       else
  //       {
  //         // Добавляем в пустые слоты
  //         for (int i = 0; i < simulatedInventory.Count; i++)
  //         {
  //           if (simulatedInventory[i].item == null && quantity > 0)
  //           {
  //             simulatedInventory[i] = new InventorySlot(item);
  //             quantity--;
  //           }
  //         }

  //         // Если остались предметы, добавляем в конец
  //         while (quantity > 0 && simulatedInventory.Count < simulatedSlotLimit)
  //         {
  //           simulatedInventory.Add(new InventorySlot(item));
  //           quantity--;
  //         }
  //       }
  //     }
  //   }

  //   // Хватает места для всех наград
  //   return true;
  // }

  //Метод получения кол-ва пустых слотов
  private int GetEmptySlotsCount()
  {
    int emptySlots = 0;
    foreach (var slot in items)
    {
      if (slot.item == null)
      {
        emptySlots++;
      }
    }
    return emptySlots;
  }
}