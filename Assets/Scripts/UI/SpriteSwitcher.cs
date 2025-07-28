using UnityEngine;
using UnityEngine.UI;

public class SpriteSwitcher : MonoBehaviour
{
    [Tooltip("Первый Image компонент")]
    public Image image1;

    [Tooltip("Второй Image компонент")]
    public Image image2;

    // Валидация полей в редакторе.  Опционально, но полезно.
    private void OnValidate()
    {
        //Попытаемся найти эти Image объекты, если они не установлены
        if (image1 == null)
        {
            Debug.LogWarning("Image1 не назначен, пытаюсь найти...");
            image1 = transform.Find("Image1")?.GetComponent<Image>(); //Замените Image1 на имя вашего первого объекта
            if (image1 == null)
            {
                Debug.LogError("Image1 не найден! Убедитесь, что у вас есть дочерний объект Image с именем Image1.");
            }
        }

        if (image2 == null)
        {
            Debug.LogWarning("Image2 не назначен, пытаюсь найти...");
            image2 = transform.Find("Image2")?.GetComponent<Image>(); //Замените Image2 на имя вашего второго объекта
            if (image2 == null)
            {
                Debug.LogError("Image2 не найден! Убедитесь, что у вас есть дочерний объект Image с именем Image2.");
            }
        }
    }

    public void UpdateSpriteStates()
    {
        if (image1 == null || image2 == null)
        {
            Debug.LogError("Одна или несколько ссылок на Image компоненты не назначены!");
            return;
        }

        bool hasSprite1 = image1.sprite != null;
        bool hasSprite2 = image2.sprite != null;

        if (hasSprite2)
        {
            image2.gameObject.SetActive(true); // Включаем второй Image
            image1.gameObject.SetActive(false); // Отключаем первый Image
        }
        else if (hasSprite1)
        {
            image1.gameObject.SetActive(true); // Включаем первый Image
            image2.gameObject.SetActive(false); // Отключаем второй Image
        }
        else
        {
            image1.gameObject.SetActive(false); // Отключаем первый Image
            image2.gameObject.SetActive(false); // Отключаем второй Image
        }
    }
}


// Use in other script
// public SpriteSwitcher spriteSwitcher;
// spriteSwitcher.UpdateSpriteStates();