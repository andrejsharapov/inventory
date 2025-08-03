using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private TextMeshProUGUI _quantityText;
    [SerializeField] private Image _itemIcon;
    [SerializeField] private Image _backgroundIcon;
    [SerializeField] private Image _slotIcon;

    [Space, Header("Sprites")]
    [SerializeField] private Sprite _defaultSlot;
    [SerializeField] private Sprite _selectedSlot;
    [SerializeField] private Sprite _tooltipIcon;

    private Image _selfImage;

    private InventorySlot _inventorySlot;
    private InventoryView _inventoryView;

    private Coroutine _longPressCoroutine;
    private const float LongPressDuration = 0.6f;

    public InventorySlot InventorySlot => _inventorySlot;



    private void OnValidate()
    {
        var itemConfig = _inventorySlot?.Item?.ItemConfig;

        _itemIcon.sprite = itemConfig ? itemConfig.icon : _tooltipIcon;
        _itemIcon.enabled = _itemIcon.sprite != null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_inventorySlot?.Item == null)
            return;

        _inventoryView.SetSelectedSlot(this);
        DraggableIcon.Instance.Show(_inventorySlot.Item, this, eventData.position);
        _itemIcon.enabled = false;
        HideModal();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_inventorySlot?.Item == null)
            return;
        DraggableIcon.Instance.Move(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DraggableIcon.Instance.Hide();
        if (_inventorySlot?.Item == null)
            return;
        _inventoryView.SetSelectedSlot(this);
        HideModal();
        _itemIcon.enabled = _itemIcon.sprite;
    }

    public void Initialize(InventorySlot inventorySlot, InventoryView inventoryView)
    {
        _inventorySlot = inventorySlot;
        _inventoryView = inventoryView;

        _selfImage = GetComponent<Image>();
    }

    public void SetData(InventorySlot slot)
    {
        var itemConfig = slot.Item?.ItemConfig;

        _quantityText.text = slot.Quantity.ToString();
        _quantityText.enabled = itemConfig && itemConfig.isStackable;
        
        _itemIcon.sprite = itemConfig ? itemConfig.icon : _tooltipIcon;
        _itemIcon.enabled = _itemIcon.sprite != null;

        _backgroundIcon.sprite = itemConfig?.slotBackground ?? null;
        _backgroundIcon.enabled = _backgroundIcon.sprite != null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggedItem = DraggableIcon.Instance.DraggableItem;
        var sourceSlotView = DraggableIcon.Instance.SourceSlot;
        DraggableIcon.Instance.Hide();
        if (draggedItem == null || sourceSlotView == null)
            return;

        if (ReferenceEquals(sourceSlotView, this))
            return;

        if (_inventorySlot is EquipmentSlot equipmentSlot && (draggedItem is EquipableItem == false || draggedItem.ItemConfig?.equipmentType != equipmentSlot.SlotType))
            return; // Проверка -> Может ли переносимый нами предмет лечь в эквип ячейку

        if (_inventorySlot.Item != null && sourceSlotView.InventorySlot is EquipmentSlot sourceEquipmentSlot && (_inventorySlot.Item is EquipableItem == false || _inventorySlot.Item.ItemConfig?.equipmentType != sourceEquipmentSlot.SlotType))
            return; // Проверка -> Может ли предмет в ячейке перенестись в эквип ячейку

        _inventoryView.SetSelectedSlot(this);
        _inventorySlot.Inventory.MoveOrSwapItems(sourceSlotView.InventorySlot, _inventorySlot);

    }

    public void SetSelected(bool selected)
    {
        _slotIcon.sprite = selected ? _selectedSlot : _defaultSlot;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            OnDoubleClick();
        }
    }

    private void OnDoubleClick()
    {
        _inventoryView.OnUseInput(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        var isHasItem = _inventorySlot?.Item?.ItemConfig != null;

        _inventoryView.SetSelectedSlot(isHasItem ? this : null);

        if (_inventorySlot?.Item == null)
            return;

        _longPressCoroutine = StartCoroutine(LongPressRoutine(eventData));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        HideModal();
    }

    private void HideModal()
    {
        if (_longPressCoroutine != null)
        {
            StopCoroutine(_longPressCoroutine);
            _longPressCoroutine = null;
        }
        InventoryModalUI.Hide();
    }

    private IEnumerator LongPressRoutine(PointerEventData eventData)
    {
        float elapsed = 0;
        while (elapsed < LongPressDuration)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                (RectTransform)transform, Input.mousePosition, eventData.enterEventCamera))
            {
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        _longPressCoroutine = null;

        ShowModalRightOfSlot();
    }

    private void ShowModalRightOfSlot()
    {
        Vector3 worldPos = ((RectTransform)transform).position;
        Vector2 slotSize = ((RectTransform)transform).rect.size;
        Vector3 rightOffset = worldPos + transform.right * slotSize.x * 0.6f;

        InventoryModalUI.Show(_inventorySlot, (RectTransform)transform);
    }
}
