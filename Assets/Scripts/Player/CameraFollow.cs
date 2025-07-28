using UnityEngine;

// Rotation 40/-45/0
// Camera - Projection Orthographic Size 4
public class CameraFollow : MonoBehaviour
{
    [Tooltip("Цель, за которой будет следовать камера.")]
    public Transform target;

    [Tooltip("Смещение камеры по высоте относительно цели.")]
    public float heightOffset = 1.5f;

    [Tooltip("Расстояние камеры от цели.")]
    public float distance = 10.0f;

    [Tooltip("Скорость, с которой камера догоняет цель.  Более высокие значения приводят к более быстрому и резкому следованию.")]
    public float followSpeed = 5f;

    private readonly Quaternion initialRotation = Quaternion.identity; // Сохраняем начальное вращение камеры. Инициализация при объявлении!

    void Start()
    {
        // Инициализируем начальное вращение при старте.
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);

        // Получаем позицию цели на горизонтальной плоскости (игнорируем Y)
        Vector3 targetPositionHorizontal = new Vector3(target.position.x, 0, target.position.z);

        // Создаем желаемую позицию, смещенную назад и вверх.
        Vector3 desiredPosition = targetPositionHorizontal - transform.rotation * Vector3.forward * distance + Vector3.up * heightOffset;

        // Устанавливаем позицию камеры мгновенно
        transform.position = desiredPosition;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Получаем позицию цели на горизонтальной плоскости (игнорируем Y)
        Vector3 targetPositionHorizontal = new Vector3(target.position.x, 0, target.position.z);

        // Создаем желаемую позицию, смещенную назад и вверх.
        Vector3 desiredPosition = targetPositionHorizontal - transform.rotation * Vector3.forward * distance + Vector3.up * heightOffset;

        // Плавное перемещение камеры
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Возвращаем камере начальное вращение
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
    }

    private void OnDestroy()
    {
        target = null;
    }
}