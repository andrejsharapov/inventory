using UnityEngine;
using TMPro;

public class ItemInfoManager : MonoBehaviour
{
  private static ItemInfoManager _instance;
  public static ItemInfoManager Instance
  {
    get
    {
      if (_instance == null)
      {
        // Попытаемся найти существующий экземпляр
        _instance = FindAnyObjectByType<ItemInfoManager>();

        // Если не нашли, создаем новый
        if (_instance == null)
        {
          GameObject singleton = new GameObject(typeof(ItemInfoManager).Name);
          _instance = singleton.AddComponent<ItemInfoManager>();
          Debug.LogWarning("An instance of " + typeof(ItemInfoManager) +
                           " is needed in the scene, but there is none. " +
                           "GameObject created with empty instance.");
        }

        DontDestroyOnLoad(_instance.gameObject);
      }

      return _instance;
    }
  }

  [Header("Item Info Canvas Prefab")]
  public GameObject itemInfoCanvasPrefab; // Префаб Canvas с панелью

  private GameObject _currentItemInfoCanvas; // Текущий экземпляр Canvas
  private TextMeshProUGUI _itemNameText;
  private TextMeshProUGUI _itemDescriptionText;
  private TextMeshProUGUI _itemTypeText;

  //Больше не нужен, потому что будем искать Canvas динамически

  void Awake()
  {
    // Убедитесь, что itemInfoCanvasPrefab назначен в инспекторе
    if (itemInfoCanvasPrefab == null)
    {
      Debug.LogError("itemInfoCanvasPrefab is not assigned in the Inspector!");
    }
  }

  //Вызывается после загрузки скриптов
  void Start()
  {
    //Убедимся, что _instance правильный
    if (_instance == null)
    {
      _instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else if (_instance != this)
    {
      Destroy(gameObject);
    }
  }

  public void ShowItemInfo(Item item, Transform targetTransform)
  {
    HideItemInfo();

    if (itemInfoCanvasPrefab == null)
    {
      Debug.LogError("itemInfoCanvasPrefab is not assigned!  Cannot show item info.");
      return;
    }

    _currentItemInfoCanvas = Instantiate(itemInfoCanvasPrefab);
    RectTransform panelRect = _currentItemInfoCanvas.transform.Find("Panel").GetComponent<RectTransform>();
    RectTransform targetRect = targetTransform as RectTransform;

    //Кешируем компоненты сразу после создания панели
    _itemNameText = _currentItemInfoCanvas.transform.Find("Panel/ItemNameText").GetComponent<TextMeshProUGUI>();
    _itemDescriptionText = _currentItemInfoCanvas.transform.Find("Panel/ItemDescriptionText").GetComponent<TextMeshProUGUI>();
    _itemTypeText = _currentItemInfoCanvas.transform.Find("Panel/ItemTypeText").GetComponent<TextMeshProUGUI>();

    //Ищем Canvas вверх по иерархии от targetTransform
    Canvas targetCanvas = targetTransform.GetComponentInParent<Canvas>();

    if (targetCanvas == null)
    {
      Debug.LogError("No Canvas found in parent of targetTransform!");
      Destroy(_currentItemInfoCanvas);
      return;
    }

    _currentItemInfoCanvas.transform.SetParent(targetCanvas.transform, false);

    // === Позиционирование панели ===

    // 1. Определяем размеры и границы объекта (в координатах Canvas)
    Vector3[] targetWorldCorners = new Vector3[4];
    targetRect.GetWorldCorners(targetWorldCorners);
    Vector2 targetBottomLeft = RectTransformUtility.WorldToScreenPoint(null, targetWorldCorners[0]);
    Vector2 targetTopRight = RectTransformUtility.WorldToScreenPoint(null, targetWorldCorners[2]);
    Vector2 targetTopLeft = RectTransformUtility.WorldToScreenPoint(null, targetWorldCorners[1]);
    Vector2 targetBottomRight = RectTransformUtility.WorldToScreenPoint(null, targetWorldCorners[3]);

    // 2. Настраиваем RectTransform панели
    panelRect.anchorMin = Vector2.zero;  // Нижний левый угол
    panelRect.anchorMax = Vector2.zero;  // Нижний левый угол

    // 3. Вычисляем размеры панели (ВАЖНО: делаем это до позиционирования)
    Vector2 panelSize = panelRect.rect.size;

    // 4. Учитываем границы Canvas
    Rect canvasRect = (targetCanvas.transform as RectTransform).rect;
    Vector2 canvasSize = new Vector2(canvasRect.width, canvasRect.height);

    // 5. Определение начальной позиции и pivot
    Vector2 panelPosition = targetTopRight;
    panelRect.pivot = new Vector2(1, 1); //Правый верхний угол

    // === Корректировка позиции ===

    // Проверяем, помещается ли панель сверху
    if (targetTopRight.y + panelSize.y > canvasSize.y / 2)
    {
      // Если не помещается сверху, размещаем снизу
      panelPosition = targetBottomRight;
      panelRect.pivot = new Vector2(1, 0); // правый нижний угол
    }

    // Проверяем, помещается ли панель справа
    if (panelPosition.x - panelSize.x < -canvasSize.x / 2)
    {
      // Если не помещается справа, размещаем слева
      if (panelRect.pivot.y == 1)
      {
        // Если панель сверху
        panelPosition = targetTopLeft;
        panelRect.pivot = new Vector2(0, 1); // левый верхний угол
      }
      else
      {
        // Если панель снизу
        panelPosition = targetBottomLeft;
        panelRect.pivot = new Vector2(0, 0); // левый нижний угол
      }
    }
    if (panelPosition.y - panelSize.y < -canvasSize.y / 2)
    {
      if (panelRect.pivot.x == 1)
      { //Если панель справа
        panelPosition = targetTopRight;
        panelRect.pivot = new Vector2(1, 1); // правый верхний угол
      }
      else
      { // если панель слева
        panelPosition = targetTopLeft;
        panelRect.pivot = new Vector2(0, 1); // левый верхний угол
      }
    }

    // 6. Применяем позицию
    panelRect.position = panelPosition;

    // Устанавливаем текст
    _itemNameText.text = item.itemName;
    _itemDescriptionText.text = item.description;
    _itemTypeText.text = item.itemType.ToString();

    // Активируем Canvas
    _currentItemInfoCanvas.SetActive(true);
  }

  public void HideItemInfo()
  {
    Destroy(_currentItemInfoCanvas?.gameObject);
    _currentItemInfoCanvas = null;
  }
}