using UnityEngine;
using UnityEngine.UI;

public class DraggableIcon : MonoBehaviour
{
    public static DraggableIcon Instance { get; private set; }

    [SerializeField] private Image _iconImage;
    [SerializeField] private CanvasGroup _canvasGroup;

    public InventoryItem DraggableItem { get; private set; }
    public InventorySlotView SourceSlot { get; private set; }

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(InventoryItem inventoryItem, InventorySlotView sourceSlot, Vector2 position, Canvas parentCanvas = null)
    {
        DraggableItem = inventoryItem;
        SourceSlot = sourceSlot;
        _iconImage.sprite = inventoryItem?.ItemConfig?.icon;
        _canvasGroup.alpha = 1f;

        if(parentCanvas != null)
        transform.SetParent(parentCanvas.transform, false);
        Move(position);
    }

    public void Move(Vector2 position)
    {
        _iconImage.transform.position = position;
    }

    public void Hide()
    {
        DraggableItem = null;
        SourceSlot = null;
        _iconImage.sprite = null;
        _canvasGroup.alpha = 0f;
    }
}