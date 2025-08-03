using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class InventoryView : MonoBehaviour
{
    [SerializeField] private GameObject _inventoryObject;

    [Space]
    [SerializeField] private InventorySlotView[] _equipmentSlotViews;

    [Space]
    [SerializeField] private InventorySlotView _slotPrefab;
    [SerializeField] private Transform _slotsParent;

    [Space]
    [SerializeField] private Button _openButton;
    [SerializeField] private Button _closeButton;

    [Space]
    [SerializeField] private Button _useButton;
    [SerializeField] private Button _splitButton;
    [SerializeField] private Button _dropButton;
    [SerializeField] private Button _unEquipButton;

    private Inventory _inventory;
    private Equipment _equipment;

    private int? _selectedIndex = null;
    private List<InventorySlotView> _slotViews;

    public InventorySlot SelectedSlot => (_selectedIndex != null && _selectedIndex >= 0 && _selectedIndex < _slotViews.Count)
        ? _slotViews[_selectedIndex.Value].InventorySlot
        : null;

    public event Action<InventorySlot> SelectedSlotChanged;

    private static readonly HashSet<Item.ItemType> UsableTypes = new()
    {
        Item.ItemType.Energy,
        Item.ItemType.Healing,
        Item.ItemType.Recipe,
        Item.ItemType.Scrap
    };

    public void Initialize(Inventory inventory)
    {
        SetInventoryOpen(false);

        _inventory = inventory;
        _equipment = inventory.Equipment;

        _slotViews = new();

        SetEquipmentSlots();

        DrawInvenorySlots();
        Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        SelectedSlotChanged += OnSelectedSlotChanged;
        _inventory.InventoryChanged += OnInventoryChanged;

        _openButton.onClick.AddListener(() => SetInventoryOpen(true));
        _closeButton.onClick.AddListener(() => SetInventoryOpen(false));

        _useButton.onClick.AddListener(() => OnUseInput(false));
        _splitButton.onClick.AddListener(OnSplitInput);
        _dropButton.onClick.AddListener(OnDropInput);
        _unEquipButton.onClick.AddListener(OnUnEquipButton);
    }

    public void SetInventoryOpen(bool open)
    {
        _inventoryObject.SetActive(open);
    }

    private void Unsubscribe()
    {
        SelectedSlotChanged -= OnSelectedSlotChanged;
        _inventory.InventoryChanged -= OnInventoryChanged;
    }

    private void OnSelectedSlotChanged(InventorySlot slot)
    {
        var itemConfig = slot?.Item?.ItemConfig;

        bool isUsable = itemConfig && UsableTypes.Contains(itemConfig.itemType);

        _useButton.interactable = itemConfig && (isUsable || slot?.Item is EquipableItem) && (slot is EquipmentSlot == false || isUsable);
        _splitButton.interactable = itemConfig && itemConfig.isStackable && slot.Quantity > 1 && slot is EquipmentSlot == false && _inventory.HasSpace();
        _dropButton.interactable = itemConfig && (slot.Item is not BagItem || (slot.Item is BagItem && _inventory.CanRemoveBag(slot.Item)));

        _unEquipButton.interactable = itemConfig && SelectedSlot is EquipmentSlot && _inventory.HasSpaceFor(itemConfig);
    }

    private void OnInventoryChanged()
    {
        DrawInvenorySlots();
    }

    private void DrawInvenorySlots()
    {
        DestroySlots();


        SetEquipmentSlots();

        foreach (var slot in _inventory.Slots)
        {
            var slotView = SpawnSlotView(_slotsParent);
            SetSlotView(slot,slotView);
        }

        SelectedSlotChanged?.Invoke(SelectedSlot);
    }
    

    private void SetEquipmentSlots()
    {
        for (int i = 0; i < Mathf.Min(_equipment.Slots.Count, _equipmentSlotViews.Length); i++)
        {
            var view = _equipmentSlotViews[i];
            var slot = _equipment.Slots[(Item.EquipmentType)i];

            SetSlotView(slot, view);
        }
    }

    private InventorySlotView SpawnSlotView(Transform parent) => Instantiate(_slotPrefab, parent);
    
    private void SetSlotView(InventorySlot slot, InventorySlotView slotView)
    {
        _slotViews.Add(slotView);
        slotView.Initialize(slot, this);
        slotView.SetData(slot);
        slotView.SetSelected(SelectedSlot == slot);
    }

    private void DestroySlots()
    {
        foreach (Transform child in _slotsParent.transform)
        {
            Destroy(child.gameObject);
        }

        _slotViews.Clear();
    }


    private void OnUnEquipButton()
    {
        if (SelectedSlot == null)
            return;

        var (success, slot) = _inventory.AddItem(SelectedSlot);

        if (success == false)
            return;

        _equipment.Unequip(SelectedSlot as EquipmentSlot);

        var slotView = _slotViews.FirstOrDefault(p => p.InventorySlot == slot);
        SetSelectedSlot(slotView);
    }

    private void OnDropInput()
    {
        if (SelectedSlot == null)
            return;

        _inventory.RemoveItemFromSlot(SelectedSlot, SelectedSlot.Quantity);

        SetSelectedSlot(null);
    }

    private void OnSplitInput()
    {
        if (SelectedSlot == null)
            return;
        
        _inventory.SplitSlot(SelectedSlot);
    }

    public void OnUseInput(bool isDoubleClicked = false)
    {
        if (SelectedSlot == null || SelectedSlot.Item?.ItemConfig == null || (!UsableTypes.Contains(SelectedSlot.Item.ItemConfig.itemType) && SelectedSlot.Item is EquipableItem == false))
            return;

        var isUsable = UsableTypes.Contains(SelectedSlot.Item.ItemConfig.itemType);

        var successfull = SelectedSlot.Item.Use();

        if (SelectedSlot.Item is EquipableItem == false || (!isDoubleClicked && isUsable))
        {
            _inventory.RemoveItemFromSlot(SelectedSlot, 1);

            if (SelectedSlot.Item == null)
                SetSelectedSlot(null);
        }
        else
        {
            var slot = SelectedSlot;
            bool isEquiped = slot is EquipmentSlot;

            if (!isEquiped) 
            {
                var equipmentSlot = _equipment.GetEquipmentSlot(slot.Item.ItemConfig.equipmentType);
                _inventory.MoveOrSwapItems(slot, equipmentSlot);
                SetSelectedSlot(_slotViews.FirstOrDefault(p => p.InventorySlot == equipmentSlot));
            }
            else
            {
                OnUnEquipButton();
            }
        }
    }

    public void SetSelectedIndex(int index)
    {
        if (_slotViews == null || _slotViews.Count == 0)
            return;

        if (_selectedIndex == index)
            return;

        if (_selectedIndex >= 0 && _selectedIndex < _slotViews.Count)
            _slotViews[_selectedIndex.Value].SetSelected(false);

        _selectedIndex = index;

        if (_selectedIndex >= 0 && _selectedIndex < _slotViews.Count)
            _slotViews[_selectedIndex.Value].SetSelected(true);

        SelectedSlotChanged?.Invoke(SelectedSlot);
    }

    public int GetIndexBySlot(InventorySlotView inventorySlot) => Array.IndexOf(_slotViews.ToArray(), inventorySlot);

    public void SetSelectedSlot(InventorySlotView inventorySlotView)
    {
        if (_slotViews == null || inventorySlotView == null)
        {
            ClearSelection();
            return;
        }

        int index = Array.IndexOf(_slotViews.ToArray(), inventorySlotView);

        if (index < 0)
            return;

        SetSelectedIndex(index);
    }
    public void ClearSelection()
    {
        if (_selectedIndex.HasValue && _selectedIndex < _slotViews.Count)
            _slotViews[_selectedIndex.Value].SetSelected(false);

        _selectedIndex = null;
        SelectedSlotChanged?.Invoke(null);
    }
}
