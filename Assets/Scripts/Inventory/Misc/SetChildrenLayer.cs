using UnityEngine;

[DisallowMultipleComponent]
public sealed class SetChildrenLayer : MonoBehaviour
{
    [SerializeField, Range(0, 31)]
    private int targetLayer = 0;

    [SerializeField]
    private bool includeSelf = false;

    private void Awake()
    {
        var transforms = GetComponentsInChildren<Transform>(includeInactive: true);
        for (int i = 0; i < transforms.Length; i++)
        {
            var t = transforms[i];
            if (!includeSelf && t == transform)
                continue;

            t.gameObject.layer = targetLayer;
        }

        enabled = false;
    }

    private void OnValidate()
    {
        if (targetLayer < 0 || targetLayer > 31)
            targetLayer = Mathf.Clamp(targetLayer, 0, 31);
    }
}