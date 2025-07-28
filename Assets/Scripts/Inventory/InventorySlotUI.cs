using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
  public TextMeshProUGUI countText;

  [Header("Slot")]
  public Image background;
  public Sprite defaultSprite;
  public Sprite selectedSprite;
  public Sprite unavailableSprite;

  [Header("Item")]
  public Image icon;
  public Image iconBackground;

  public InventorySlot slot;
  public Item item;
  private InventoryUI inventoryUI;

  void Start()
  {
    if (transform.root.GetComponentInChildren<InventoryUI>() != null)
    {
      inventoryUI = transform.root.GetComponentInChildren<InventoryUI>();
    }
    else
    {
      Debug.Log("Can`t find InventoryUI");
    }
  }

  // Заполняет слот предметом
  public void AddItem(InventorySlot inventorySlot, InventoryUI ui)
  {
    inventoryUI = ui;
    item = inventorySlot.item;
    slot = inventorySlot;

    icon.sprite = item.icon;
    icon.enabled = true;

    if (item.isStackable)
    {
      countText.text = slot.count.ToString();
      countText.enabled = true;
    }
    else
    {
      countText.enabled = false;
    }

    SetSlotAvailability(true); // Устанавливаем доступность
    DeselectSlot();
  }

  // Очищает слот
  public void ClearSlot(InventoryUI ui)
  {
    inventoryUI = ui;
    item = null;
    icon.sprite = null;
    icon.enabled = false;
    countText.enabled = false;
    countText.text = "";

    SetSlotAvailability(false); // Устанавливаем недоступность
    DeselectSlot();
  }

  // Вызывается при нажатии на слот
  public void OnSlotClick()
  {
    // Получаем индекс слота из иерархии
    Transform itemsParent = transform.parent; // Получаем родительский объект (itemsParent)
    int slotIndex = transform.GetSiblingIndex(); // Получаем индекс слота среди дочерних элементов

    if (item != null)
    {
      inventoryUI.SetItemToRemove(item, this, slotIndex); // Выбираем предмет для разделения
    }
  }

  // Метод для отрисовки выбранного слота
  public void SelectSlot()
  {
    background.sprite = selectedSprite;
  }

  // Метод для отрисовки стандартного слота
  public void DeselectSlot()
  {
    if (item != null)
    {
      SetSlotAvailability(true);
    }
  }

  // Метод для установки состояния слота (доступен/недоступен)
  public void SetSlotAvailability(bool isAvailable)
  {
    if (isAvailable)
    {
      background.sprite = defaultSprite; // Используем defaultSprite для доступности слота

      if (item != null)
      {
        if (item.itemRarity == Item.ItemRarity.Common) // Проверяем ранг
        {
          iconBackground.sprite = defaultSprite; // Убираем фон для низкого ранга
        }
        else if (item.slotBackground != null)
        {
          iconBackground.sprite = item.slotBackground; // Устанавливаем slotBackground для других рангов
        }
      }
    }
    else
    {
      background.sprite = unavailableSprite; // Используем unavailableSprite для недоступности слота
      iconBackground.sprite = unavailableSprite; // Если предмет null, убираем фон
    }
  }
}