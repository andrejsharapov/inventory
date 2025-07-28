using UnityEngine;
using UnityEngine.EventSystems;

public class RewardItemHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
  public Item item; // Item, связанный с этим элементом UI

  public void OnPointerDown(PointerEventData eventData)
  {
    // Вызываем ShowItemInfo из ItemInfoManager напрямую
    ItemInfoManager.Instance.ShowItemInfo(item, transform); // Передаем transform
  }

  public void OnPointerUp(PointerEventData eventData)
  {
    ItemInfoManager.Instance.HideItemInfo();
  }
}