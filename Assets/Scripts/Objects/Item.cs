using UnityEngine;
using System.Collections.Generic;

// IMPORTANT При добавлении полей , обязательно внести изменения в ItemTypeEditor в папке Editor
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public enum ItemType { Default, Weapon, Armor, Bag, Consumable, Energy, Healing, Scrap, Recipe, Building } // Типы предметов
    public enum ItemRarity { Common, Uncommon, Rare, Legendary, Epic } // Ранги предметов
    public enum ItemRarityName { Обычный, Редкий, Уникальный, Легендарный, Эпический } // Ранги предметов
    public enum DamageType { Physical, Fire, Water, Earth, Air, Light, Dark } // Типы урона
    public enum WeaponType { OneHand, TwoHand, Bow, CrossBow, Shield } //  Подтип для оружия
    public enum ArmorType { Helmet, Chest, Gloves, Legs, Boots } // Подтип для брони
    public enum EquipmentType { Weapon, Sheild, RangeWeapon, Recipe, Food, Helmet, Chest, Gloves, Legs, Boots } // Все слоты эквипа
    public enum CraftCategory { None, WeaponArmor, FoodDrink, Potions, Spells, Buildings } // Категории для крафта
    public enum GameBookCategory { None, Bestiary, Story }

    [Header("Item Type")]
    public ItemType itemType; // Тип предмета (enum)
    public EquipmentType equipmentType = EquipmentType.Weapon;

    [Header("Bag size")]
    public int bagCapacity = 0; // Вместимость сумки

    [Header("Stack")]
    public bool isStackable = false;
    public int maxStackSize = 1;  // Максимальное количество предметов в стаке

    [Header("Item info")]
    public string itemName = "";
    public GameObject prefab = null; // Префаб предмета (для отображения в мире)
    public Sprite icon = null;
    [TextArea(3, 10)] // Для многострочного описания в инспекторе
    public string description;

    [Header("Item Range")]
    public int itemRarityID; // ID ранга
    public ItemRarityName itemRarityName; // Название ранга
    public ItemRarity itemRarity; // Ранг предмета
    public Sprite slotBackground; // Спрайт фона слота

    [Header("Healing Stats")]
    public int healthRestore = 0; // Значение восстановления здоровья
    public int hungerRestore = 0; // Значение восстановления голода
    public int thirstRestore = 0; // Значение восстановления жажды
    public int manaRestore = 0; //Значение восстановления маны
    public int energyRestore = 0; // Значение восстановления энергии

    [Header("Weapon Stats")]
    public int damage = 2; // Урон (базовый, физический)
    public DamageType damageType = DamageType.Physical; // Тип урона (стихия)
    public int elementalDamage = 0; // Значение стихийного урона
    [Range(0.1f, 5f)] // Ограничиваем скорость атаки разумными значениями
    public float attackSpeed = 1f; // Скорость атаки (атак в секунду)
    public int durability = 100000; // Прочность
    public float range = 1.1f; // Дальность
    public int enemyCount = 1; // Сколько целей поражает оружие
    public WeaponType weaponType = WeaponType.OneHand;
    public ArmorType armorType = ArmorType.Helmet;

    [Header("Craft Recipe")]
    public int craftLevel = 1;
    public CraftCategory craftCategory = CraftCategory.None;
    public Item craftItem;
    public List<Ingredient> ingredients;

    [Header("Game Book")]
    public GameBookCategory gameBookCategory = GameBookCategory.None;
    [TextArea(3, 10)]
    public string gameBookDescription; // Описание для GameBook
    public Sprite gameBookIcon; // Иконка для GameBook

    [System.Serializable]
    public class Ingredient
    {
        public Item item; // Ссылка на Item (ингредиент)
        public int count = 1; // Количество необходимого ингредиента
    }
    
    public virtual void Use(GameObject user)
    {
        //     // Базовая реализация Use() (может быть переопределена в дочерних классах)
        //     Debug.Log("Using: " + itemName);

        //     // Добавляем логику для типа Healing
        //     if (itemType == ItemType.Healing)
        //     {
        //         Debug.Log("Used a healing item: " + itemName);

        //         // Получаем компонент Health у пользователя
        //         Health health = user.GetComponent<Health>();

        //         if (health != null)
        //         {
        //             health.Heal(healthRestore); // Исцеляем игрока
        //             Hunger.Instance.IncreaseHunger(hungerRestore); // Восполняем голод
        //             Thirst.Instance.IncreaseThirst(thirstRestore); // Восполняем жажду
        //             EnergyManager.Instance.UseEnergy(energyRestore); // Восполняем энергию
        //         }
        //         else
        //         {
        //             Debug.LogWarning("No Health component found on user: " + user.name);
        //         }
        //     }
    }
}