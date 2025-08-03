using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerRotate : MonoBehaviour,IDragHandler
{
    [SerializeField] private float _rotateSpeed = 0.1f;
    [SerializeField] private Transform _playerModel;

    public void OnDrag(PointerEventData eventData)
    {
        var model = _playerModel;
        var delta = eventData.delta.x;

        model.Rotate(-Vector3.up * delta * _rotateSpeed);
    }
}
