using Riten.Native.Cursors;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class CursorOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Cursor")]
    [SerializeField]
    private NTCursors hoverCursor = NTCursors.Link;
    private Selectable selectable;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable())
            return;

        CursorStack.Push(hoverCursor);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        CursorStack.Pop();
    }

    private bool IsInteractable() => selectable != null && selectable.IsInteractable();
}
