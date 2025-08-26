using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class SupplyCameraForCanvas : MonoBehaviour
{
    private void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
    }
}
