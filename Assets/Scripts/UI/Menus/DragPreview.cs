using UnityEngine;
using UnityEngine.EventSystems;

public class DragPreview : MonoBehaviour, IDragHandler
{
    [Header("Target")]
    [SerializeField]
    private GameObject previewObject;

    [Header("Configs")]
    [SerializeField]
    private float sensitivity = 1f;

    public void OnDrag(PointerEventData eventData)
    {
        if (previewObject == null)
            return;

        previewObject.transform.eulerAngles += new Vector3(-eventData.delta.y * sensitivity, 0);
    }
}
