using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDescriptionUI : MonoBehaviour
{
  public TextMeshProUGUI itemNameText;
  public TextMeshProUGUI itemDescriptionText;
  public GameObject itemDescriptionPanel;

  // Метод для отображения информации о предмете
  public void ShowItemDescription(Item item)
  {
    if (item != null)
    {
      itemNameText.text = item.itemName;
      itemDescriptionText.text = item.description;
      itemDescriptionPanel.SetActive(true); // Включаем панель с описанием
    }
    else
    {
      ClearItemDescription();
    }
  }

  // Метод для очистки информации о предмете и скрытия панели
  public void ClearItemDescription()
  {
    itemNameText.text = "";
    itemDescriptionText.text = "";
    itemDescriptionPanel.SetActive(false); // Выключаем панель с описанием
  }
}