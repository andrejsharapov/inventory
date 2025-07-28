using UnityEngine;
using UnityEngine.UI;

public class WeaponDisplay : MonoBehaviour
{
    [Header("Слоты для отображения")]
    public Image weaponSlotImage; // Ссылка на Image для оружия (ближний бой)
    public Image bowSlotImage; // Ссылка на Image для лука/арбалета
    public Image shieldSlotImage; // Ссылка на Image для щита

    [Header("Быстрые слоты")]
    public Image quickSlotWeapon; // Ссылка на Image для быстрого слота оружия
    public Image quickSlotBow; // Ссылка на Image для быстрого слота лука/арбалета
    public Image quickSlotShield; // Ссылка на Image для быстрого слота щита

    [Header("Объекты для отображения")]
    [Tooltip("Parent object containing all one-handed weapon prefabs.")]
    public GameObject spineOneHand; // Ссылка на Spine_OneHand.
    [Tooltip("Parent object containing all two-handed weapon prefabs.")]
    public GameObject spineTwoHand; // Ссылка на Spine_TwoHand.
    [Tooltip("Parent object containing all bow prefabs.")]
    public GameObject spineBow; // Ссылка на Spine_Bow
    [Tooltip("Parent object containing all shield prefabs.")]
    public GameObject spineShield; // Ссылка на Spine_Shield.

    [Tooltip("Quiver object to activate when a bow/crossbow is equipped.")]
    public GameObject quiver; // Ссылка на Quiver GameObject. Перетащи сюда Quiver GameObject из иерархии.

    private Sprite _currentWeaponSprite;
    private GameObject _currentWeaponPrefab; // Ссылка на текущий префаб оружия

    private Sprite _currentBowSprite;
    private GameObject _currentBowPrefab; // Ссылка на текущий префаб лука/арбалета

    private Sprite _currentShieldSprite;
    private GameObject _currentShieldPrefab; // Ссылка на текущий префаб щита

    // Метод для отображения оружия.  Вызывается из InventoryUI
    public void DisplayWeapon(Item weaponItem)
    {
        Sprite weaponSprite = null;
        if (weaponItem != null)
        {
            weaponSprite = weaponItem.icon;
        }

        // Отображаем иконку в UI
        DisplaySprite(weaponSprite, weaponSlotImage, ref _currentWeaponSprite);

        // Отображаем иконку в быстром слоте
        DisplaySprite(weaponSprite, quickSlotWeapon, ref _currentWeaponSprite);

        // Активируем префаб
        if (weaponItem != null)
        {
            if (weaponItem.weaponType == Item.WeaponType.OneHand)
            {
                ActivatePrefab(weaponItem, spineOneHand, ref _currentWeaponPrefab);
            }
            else if (weaponItem.weaponType == Item.WeaponType.TwoHand)
            {
                ActivatePrefab(weaponItem, spineTwoHand, ref _currentWeaponPrefab);
            }
            else
            {
                Debug.LogError("WeaponDisplay: Invalid weapon type for melee weapon: " + weaponItem.weaponType);
            }
        }
        else
        {
            ClearPrefab(ref _currentWeaponPrefab);
        }
    }

    public void DisplayBow(Item bowItem)
    {
        Sprite bowSprite = null;
        if (bowItem != null)
        {
            bowSprite = bowItem.icon;
        }
        // Отображаем иконку в UI
        DisplaySprite(bowSprite, bowSlotImage, ref _currentBowSprite);

        // Отображаем иконку в быстром слоте
        DisplaySprite(bowSprite, quickSlotBow, ref _currentBowSprite);

        // Активируем префаб
        ActivatePrefab(bowItem, spineBow, ref _currentBowPrefab);

        //Активируем колчан
        ActivateQuiver(bowItem != null);
    }

    public void DisplayShield(Item shieldItem)
    {
        Sprite shieldSprite = null;
        if (shieldItem != null)
        {
            shieldSprite = shieldItem.icon;
        }
        // Отображаем иконку в UI
        DisplaySprite(shieldSprite, shieldSlotImage, ref _currentShieldSprite);

        // Отображаем иконку в быстром слоте
        DisplaySprite(shieldSprite, quickSlotShield, ref _currentShieldSprite);

        // Активируем префаб
        ActivatePrefab(shieldItem, spineShield, ref _currentShieldPrefab);
    }

    // Общий метод для отображения спрайта в слоте
    private void DisplaySprite(Sprite sprite, Image slotImage, ref Sprite currentSprite)
    {
        if (slotImage == null)
        {
            Debug.LogError("WeaponDisplay: slotImage is not assigned!  Assign the Image component in the Inspector.");
            return;
        }

        if (sprite == null)
        {
            // Если спрайт null, очищаем слот.
            slotImage.sprite = null;
            slotImage.enabled = false; // Скрываем Image, если оружия нет.
            currentSprite = null;
            return;
        }

        slotImage.sprite = sprite;
        slotImage.enabled = true; // Показываем Image, если есть оружие.
        currentSprite = sprite;
    }

    // Общий метод для активации префаба
    private void ActivatePrefab(Item item, GameObject parentObject, ref GameObject currentPrefab)
    {
        // Сначала деактивируем текущий префаб (если есть)
        ClearPrefab(ref currentPrefab);

        if (item != null && item.icon != null)
        {
            if (parentObject == null)
            {
                Debug.LogError("WeaponDisplay: parentObject is not assigned!");
                return;
            }

            if (item != null && item.prefab != null)
            {
                // Ищем префаб в родительском объекте по имени
                GameObject prefab = FindChildPrefabRecursive(parentObject.transform, item.prefab.name);

                if (prefab != null)
                {
                    // Активируем префаб
                    prefab.SetActive(true);
                    currentPrefab = prefab;
                }
                else
                {
                    Debug.LogError("WeaponDisplay: Prefab not found in parentObject for sprite: " + item.icon.name);
                }
            }
            else
            {
                Debug.LogError("WeaponDisplay: Prefab not found for sprite: " + item.icon.name);
            }
        }
    }

    // Рекурсивный метод для поиска префаба по имени
    private GameObject FindChildPrefabRecursive(Transform parent, string prefabName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == prefabName)
            {
                return child.gameObject;
            }

            // Рекурсивный вызов для поиска в дочерних элементах
            GameObject found = FindChildPrefabRecursive(child, prefabName);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    // Общий метод для деактивации префаба
    private void ClearPrefab(ref GameObject currentPrefab)
    {
        if (currentPrefab != null)
        {
            currentPrefab.SetActive(false);
            currentPrefab = null;
        }
    }

    // Метод для активации/деактивации Quiver
    private void ActivateQuiver(bool activate)
    {
        if (quiver != null)
        {
            quiver.SetActive(activate);
        }
    }

    // Метод для очистки слота оружия (например, когда оружие убирается).
    public void ClearWeaponSlot()
    {
        DisplayWeapon(null);
    }

    public void ClearBowSlot()
    {
        DisplayBow(null);
    }

    public void ClearShieldSlot()
    {
        DisplayShield(null);
    }

    // Возвращает true если сейчас есть оружие в слоте
    public bool HasWeapon()
    {
        return _currentWeaponSprite != null;
    }

    public bool HasBow()
    {
        return _currentBowSprite != null;
    }

    public bool HasShield()
    {
        return _currentShieldSprite != null;
    }
}