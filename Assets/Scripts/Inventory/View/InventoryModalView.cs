using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryModalView : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _description;
    [SerializeField] private TextMeshProUGUI _type;

    [Header("Settings")]
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _panel;

    private static InventoryModalView _instance;
    public static InventoryModalView Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        Hide();

    }

    public static void Show(InventorySlot slot, RectTransform slotRect)
    {
        if (_instance == null)
        {
            Debug.LogWarning("InventoryModalUI instance not found in scene.");
            return;
        }

        Instance?.InnerShow(slot, slotRect);
    }

    private void InnerShow(InventorySlot slot, RectTransform slotRect)
    {
        var item = slot.Item?.ItemConfig;
        if (item == null)
        {
            Hide();
            return;
        }
        _name.text = item.itemName ?? item.name;
        _description.text = item.description;
        _type.text = item.itemType.ToString();

        Vector3[] corners = new Vector3[4];
        slotRect.GetWorldCorners(corners);
        Vector3 slotRightTopWorld = corners[2];

        Vector2 slotScreenPoint = RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, slotRightTopWorld);
        RectTransform canvasRect = (RectTransform)_canvas.transform;

        Vector2 canvasLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, slotScreenPoint, _canvas.worldCamera, out canvasLocalPos
        );

        Vector2 modalPos = canvasLocalPos;

        _panel.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_panel);
        Vector2 panelSize = _panel.rect.size;
        Vector2 canvasSize = canvasRect.rect.size;

        float minX = -canvasSize.x * 0.5f;
        float maxX = canvasSize.x * 0.5f;
        float minY = -canvasSize.y * 0.5f;
        float maxY = canvasSize.y * 0.5f;

        if (modalPos.x + panelSize.x > maxX)
            modalPos.x = maxX - panelSize.x;
        if (modalPos.x < minX)
            modalPos.x = minX;

        if (modalPos.y - panelSize.y < minY)
            modalPos.y = minY + panelSize.y;
        if (modalPos.y > maxY)
            modalPos.y = maxY;

        _panel.anchoredPosition = modalPos;

    }

    public static void Hide()
    {
        Instance._panel.gameObject.SetActive(false);
    }
}