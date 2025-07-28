using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using UnityEngine.EventSystems; //  Для обработки событий UI (OnTouchDown и т.д.)

// TODO добавить двойной клик для переноса предметов в быстрые слоты
public class InventoryUI : MonoBehaviour
{
  // Синглтон
  public static InventoryUI instance;

  [Header("Canvas")]
  public GameObject inventoryUI; // Основной Canvas, где находится весь UI инвентаря
  public GameObject inventoryPanel; // Панель, которую нужно скрывать/показывать

  public Transform itemsParent;
  public InventorySlotUI[] slots;

  [Header("Buttons")]
  public Button openInventoryButton;
  public Button closeInventoryButton;
  public Button splitButton;
  public Button removeButton;
  public Button useButton; // Ссылка на кнопку "Применить"

  Inventory inventory;

  private Item itemToRemove;
  private InventorySlotUI selectedSlot;

  [Header("Item Description")]
  public GameObject itemDescriptionPanel;
  public TextMeshProUGUI itemNameText;
  public TextMeshProUGUI itemDescriptionText;
  public TextMeshProUGUI itemTypeText;
  private RectTransform itemDescriptionPanelRectTransform; // Добавлено для доступа к RectTransform

  [Header("Weapon")]
  public WeaponDisplay weaponDisplay; // Ссылка на WeaponDisplay на игроке
  private Item _currentWeapon; // Текущее оружие
  private Item _currentBow; // Текущий лук/арбалет
  private Item _currentShield; // Текущий щит
  public SpriteSwitcher spriteSwitcher;

  public enum WeaponType
  {
    OneHandTwoHand,
    BowCrossBow,
    Shield
  }

  [Header("!Do not change")]
  public int selectedSlotIndex = -1;

  void Awake()
  {
    // Реализация синглтона
    if (instance == null)
    {
      instance = this;
    }
    else
    {
      Destroy(gameObject); // Удаляем дубликаты
      return;
    }
  }

  void Start()
  {
    inventory = Inventory.instance;
    if (inventory == null)
    {
      Debug.LogError("Inventory instance not found!"); // Обработка ошибки: Inventory должен быть создан.
      enabled = false; // Отключаем скрипт, чтобы не было ошибок.
      return;
    }
    inventory.onInventoryChanged += UpdateUI;

    // slots = itemsParent.GetComponentsInChildren<InventorySlotUI>();
    // Изменено: Получаем слоты из Viewport.Content
    slots = itemsParent.Find("Viewport").Find("Content").GetComponentsInChildren<InventorySlotUI>();
    if (slots == null || slots.Length == 0)
    {
      Debug.LogError("Inventory slots not found! Check the hierarchy."); // Обработка ошибки: Не найдены слоты.
    }

    // Изначально скрываем только панель инвентаря
    if (inventoryPanel != null)
    {
      inventoryPanel.SetActive(false); // Скрываем панель инвентаря при старте.
    }
    else
    {
      Debug.LogError("Inventory Panel not assigned!");
    }

    removeButton.interactable = false;
    removeButton.onClick.AddListener(RemoveSelectedItem);

    splitButton.interactable = false;
    splitButton.onClick.AddListener(SplitItemStack);

    useButton.interactable = false;
    useButton.onClick.AddListener(UseSelectedItem);

    openInventoryButton.onClick.AddListener(OpenInventory);
    closeInventoryButton.onClick.AddListener(CloseInventory);

    UpdateUI();
    ClearItemDescription(); // Очищаем описание при старте

    // Кешируем ссылку на RectTransform панели описания
    itemDescriptionPanelRectTransform = itemDescriptionPanel.GetComponent<RectTransform>();
    if (itemDescriptionPanelRectTransform == null)
    {
      Debug.LogError("Item Description Panel must have a RectTransform!");
    }
  }

  public void UpdateUI()
  {
    // Изменено: Получаем слоты из Viewport.Content
    slots = itemsParent.Find("Viewport").Find("Content").GetComponentsInChildren<InventorySlotUI>();
    if (slots == null)
    {
      // Debug.LogError("Inventory slots not found! Check the hierarchy.");
      return;
    }

    for (int i = 0; i < slots.Length; i++)
    {
      // Debug.Log($"UpdateUI: Slot {i}: item = {(inventory.items.Count > i && inventory.items[i].item != null ? inventory.items[i].item.itemName : "null")}");
      if (i < inventory.items.Count)
      {
        if (inventory.items[i].item != null)
        {
          slots[i].AddItem(new InventorySlot(inventory.items[i].item, inventory.items[i].count), this);
        }
        else
        {
          slots[i].ClearSlot(this);
        }
      }
      else
      {
        slots[i].ClearSlot(this);
      }

      bool isAvailable = (i < inventory.SlotLimit); // Проверяем, доступен ли слот
      slots[i].SetSlotAvailability(isAvailable); // Устанавливаем состояние
    }
  }

  public void SplitItemStack()
  {
    if (selectedSlot == null || selectedSlot.item == null || selectedSlotIndex < 0)
    {
      Debug.LogWarning("No item selected to split!");
      return;
    }

    // Получаем предмет и слот для разделения
    Item itemToSplit = selectedSlot.item; // Предмет для разделения
    InventorySlotUI sourceSlot = selectedSlot; // Исходный слот

    int totalCount = sourceSlot.slot.count; // Общее количество
    if (totalCount <= 1)
    {
      Debug.LogWarning("Cannot split item with count <= 1");
      return;
    }

    int splitAmount = totalCount / 2; // Меньшая часть
    int remainingAmount = totalCount - splitAmount; // Большая часть

    InventorySlotUI destinationSlot = null;

    // Обновляем колличество в исходном слоте
    sourceSlot.slot.count = remainingAmount;

    // Обновляем значение в Inventory
    if (selectedSlotIndex >= 0 && selectedSlotIndex < inventory.items.Count)
    {
      inventory.items[selectedSlotIndex] = sourceSlot.slot; // Обновили значение на сцене
    }

    // Ищем свободную ячейку для перемещения
    int freeSlotIndex = FindFreeSlot();

    if (freeSlotIndex != -1) // Свободная ячейка найдена
    {
      // Есть свободный слот
      destinationSlot = slots[freeSlotIndex]; // Слот для перемещения
      InventorySlot newSlot = new InventorySlot(itemToSplit, splitAmount);

      if (freeSlotIndex < inventory.items.Count)
      {
        // Если индекс в пределах списка, заменяем существующий элемент
        inventory.items[freeSlotIndex] = newSlot;
      }
      else
      {
        // Если индекс за пределами списка, добавляем новый элемент
        inventory.items.Add(newSlot);
      }
      destinationSlot.AddItem(inventory.items[freeSlotIndex], this);
    }
    else // Нет свободной ячейки
    {
      if (inventory.items.Count < inventory.SlotLimit)
      {
        InventorySlot newSlot = new InventorySlot(itemToSplit, splitAmount);
        inventory.items.Add(newSlot);
      }
      else
      {
        Debug.Log("Нет свободных слотов в инвентаре!");
      }
    }

    UpdateUI();
    ClearItemToRemove(); //  Все переменные сбрасываются
  }

  private int FindFreeSlot()
  {
    // Проходим по всем слотам
    for (int i = 0; i < inventory.SlotLimit; i++)
    {
      // Проверяем, есть ли у нас вообще такая запись
      if (i >= inventory.items.Count)
      {
        // Это пустой слот
        return i;
      }

      // Проверяем, существует ли в слоте запись
      if (inventory.items[i].item == null)
      {
        return i;
      }
    }

    // Нет свободных слотов
    return -1;
  }

  public void RemoveSelectedItem()
  {
    if (itemToRemove == null || selectedSlotIndex < 0)
    {
      Debug.LogWarning("No item selected to remove!");
      return;
    }

    inventory.Remove(selectedSlotIndex); //  Передаем индекс слота
    removeButton.interactable = false;
    splitButton.interactable = false;
    useButton.interactable = false;
    itemToRemove = null;
    UpdateUI();
    ClearItemDescription();
  }

  public void SetItemToRemove(Item item, InventorySlotUI slot, int slotIndex)
  {
    // Если этот слот уже выбран, снимаем выделение и очищаем
    if (selectedSlot == slot)
    {
      ClearItemToRemove();
      return;
    }

    // Снимаем выделение с предыдущего слота (если он был выбран)
    if (selectedSlot != null)
    {
      selectedSlot.DeselectSlot();
    }

    selectedSlotIndex = slotIndex;
    itemToRemove = item;
    selectedSlot = slot;
    selectedSlot.SelectSlot(); // Подсвечиваем выбранный слот

    removeButton.interactable = true;

    UpdateSplitButtonInteractable();

    // Отображаем информацию о предмете
    ShowItemDescription(selectedSlot.item); //  Берем из слота

    // Проверяем тип предмета и активируем кнопку "Use" только для нужных типов
    useButton.interactable = (item != null && (
      // item.itemType == Item.ItemType.Consumable ||
      item.itemType == Item.ItemType.Energy ||
      item.itemType == Item.ItemType.Healing ||
      item.itemType == Item.ItemType.Recipe ||
      item.itemType == Item.ItemType.Scrap ||
      item.itemType == Item.ItemType.Weapon
    ));
  }

  public void ClearItemToRemove()
  {
    itemToRemove = null;
    removeButton.interactable = false;
    splitButton.interactable = false;
    useButton.interactable = false;
    selectedSlotIndex = -1;

    // Убираем выделение со слота при отмене выбора
    if (selectedSlot != null)
    {
      selectedSlot.DeselectSlot();
      selectedSlot = null;
    }

    // Очищаем информацию о предмете
    ClearItemDescription();
  }

  private void UpdateSplitButtonInteractable()
  {
    if (itemToRemove != null && itemToRemove.isStackable && selectedSlot.slot.count > 1)
    {
      if (FindFreeSlot() != -1 || inventory.items.Count < inventory.SlotLimit)
      {
        splitButton.interactable = true;
      }
      else
      {
        splitButton.interactable = false;
      }
    }
    else
    {
      splitButton.interactable = false;
    }
  }

  // Метод для использования предмета
  public void UseSelectedItem()
  {
    if (itemToRemove == null || selectedSlotIndex < 0)
    {
      Debug.LogWarning("No item selected to use!");
      return;
    }

    // Получаем ссылку на игрока
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player == null)
    {
      Debug.LogWarning("Player not found!");
      return;
    }

    if (itemToRemove != null && player != null)
    {
      if (itemToRemove.itemType == Item.ItemType.Scrap)
      {
        // GameBookUI.Instance.AddEntry(itemToRemove);
      }

      if (itemToRemove.itemType == Item.ItemType.Recipe)
      {
        // CraftingUI.Instance.LearnRecipe(itemToRemove);
      }

      if (itemToRemove.itemType == Item.ItemType.Weapon)
      {
        // Определяем, к какому типу оружия относится предмет и показываем в нужном слоте
        switch (itemToRemove.weaponType)
        {
          case Item.WeaponType.OneHand:
          case Item.WeaponType.TwoHand:
            HandleWeaponSwap(itemToRemove, WeaponType.OneHandTwoHand, ref _currentWeapon);
            break;
          case Item.WeaponType.Bow:
          case Item.WeaponType.CrossBow:
            HandleWeaponSwap(itemToRemove, WeaponType.BowCrossBow, ref _currentBow);
            break;
          case Item.WeaponType.Shield:
            HandleWeaponSwap(itemToRemove, WeaponType.Shield, ref _currentShield);
            break;
          default:
            Debug.LogError("Неизвестный тип оружия!");
            return;
        }
      }

      try
      {
        itemToRemove.Use(player); // Используем предмет
      }
      catch (Exception e)
      {
        Debug.LogError("Error using item: " + e.Message);
        return; // Выходим из метода, если произошла ошибка
      }

      // QuestManager.Instance.ItemUsed(itemToRemove, 1); // Предполагается, что используем 1 предмет за раз, если не стакаемый. Если стакаемый, то кол-во уменьшаем на 1, поэтому ставим 1.

      // Удаляем предмет из инвентаря
      if (itemToRemove.isStackable)
      {
        if (inventory.items[selectedSlotIndex].count > 1)
        {
          inventory.items[selectedSlotIndex].count--;
          // QuestManager.Instance.ItemUsed(itemToRemove, -1);  // Говорим, что один предмет убран (уменьшение стака)
          inventory.onInventoryChanged?.Invoke();
        }
        else
        {
          // QuestManager.Instance.ItemUsed(itemToRemove, 1);  // Предмет удален, поэтому уменьшаем кол-во квеста
          inventory.Remove(selectedSlotIndex);
        }
      }
      else
      {
        // QuestManager.Instance.ItemUsed(itemToRemove, 1); //Уменьшаем кол-во квеста
        inventory.Remove(selectedSlotIndex);
      }

      UpdateUI();
      ClearItemToRemove();
    }

    useButton.interactable = false;
  }

  // Функция для обработки обмена/экипировки оружия
  private void HandleWeaponSwap(Item newItem, WeaponType weaponType, ref Item currentItem)
  {
    if (weaponDisplay == null)
    {
      Debug.LogError("WeaponDisplay is not assigned!");
      return;
    }

    // Проверяем, нужно ли обменивать оружие
    switch (weaponType)
    {
      case WeaponType.OneHandTwoHand:
        if (weaponDisplay.HasWeapon())
        {
          inventory.Add(currentItem, 1);
          weaponDisplay.DisplayWeapon(newItem);
          currentItem = newItem;
        }
        else
        {
          weaponDisplay.DisplayWeapon(newItem);
          currentItem = newItem;
        }
        break;

      case WeaponType.BowCrossBow:
        if (weaponDisplay.HasBow())
        {
          inventory.Add(currentItem, 1);
          weaponDisplay.DisplayBow(newItem);
          currentItem = newItem;
        }
        else
        {
          weaponDisplay.DisplayBow(newItem);
          currentItem = newItem;
        }
        break;

      case WeaponType.Shield:
        if (weaponDisplay.HasShield())
        {
          inventory.Add(currentItem, 1);
          weaponDisplay.DisplayShield(newItem);  // Передаем Item
          currentItem = newItem;
        }
        else
        {
          weaponDisplay.DisplayShield(newItem); // Передаем Item
          currentItem = newItem;
        }
        break;
    }

    spriteSwitcher.UpdateSpriteStates();
  }

  // Отображаем информацию о предмете
  public void ShowItemDescription(Item item)
  {
    if (item == null)
    {
      ClearItemDescription();
      return;
    }

    itemNameText.text = item.itemName;
    itemDescriptionText.text = item.description;

    string itemTypeInfo = "";
    switch (item.itemType)
    {
      case Item.ItemType.Healing:
        if (item.healthRestore > 0)
        {
          itemTypeInfo += "Health: +" + item.healthRestore + "\n";
        }
        if (item.hungerRestore > 0)
        {
          itemTypeInfo += "Hunger: +" + item.hungerRestore + "\n";
        }
        if (item.thirstRestore > 0)
        {
          itemTypeInfo += "Thirst: +" + item.thirstRestore + "\n";
        }
        // if (item.manaRestore > 0)
        // {
        //     itemTypeInfo += "Mana: +" + item.manaRestore + "\n";
        // }

        // Если нет информации об исцелении, отображаем что-то по умолчанию
        if (string.IsNullOrEmpty(itemTypeInfo))
        {
          itemTypeInfo = "Provides no healing effects.";
        }

        itemTypeInfo = "\n" + itemTypeInfo;
        break;

      case Item.ItemType.Energy:
        // Собираем информацию о энергии в одну строку
        if (item.energyRestore > 0)
        {
          itemTypeInfo += "Energy: +" + item.energyRestore + "\n";
        }

        // Если нет информации об энергии, отображаем что-то по умолчанию
        if (string.IsNullOrEmpty(itemTypeInfo))
        {
          itemTypeInfo = "Provides no energy effects.";
        }

        itemTypeInfo = "\n" + itemTypeInfo;
        break;

      default:
        // Для всех остальных типов предметов просто отображаем тип
        itemTypeInfo = item.itemType.ToString();
        break;
    }

    itemTypeText.text = itemTypeInfo;
    itemDescriptionPanel.SetActive(true); // Включаем панель с описанием
  }

  // Очищаем информацию о предмете и скрываем панель
  public void ClearItemDescription()
  {
    itemNameText.text = "";
    itemDescriptionText.text = "";
    itemTypeText.text = "";
    itemDescriptionPanel.SetActive(false); // Выключаем панель с описанием
  }

  // Метод открытия инвентаря
  public void OpenInventory()
  {
    if (inventoryPanel != null)
    {
      inventoryPanel.SetActive(true); // Показываем панель инвентаря
    }
    else
    {
      Debug.LogError("Inventory Panel not assigned!");
    }
  }

  // Метод закрытия инвентаря
  public void CloseInventory()
  {
    if (inventoryPanel != null)
    {
      inventoryPanel.SetActive(false); // Скрываем панель инвентаря
    }
    else
    {
      Debug.LogError("Inventory Panel not assigned!");
    }
    ClearItemDescription();
  }
}