using UnityEngine;

public class SelectionIndicator : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private GameObject selectionIndicator;

    [Header("Configs")]
    [SerializeField]
    private Vector3 offset = new(0, 0.2f, 0);

    private void Awake()
    {
        SelectionSystem.OnSelected += HandleSelected;
        SelectionSystem.OnDeselected += HandleDeselected;
    }

    private void HandleSelected(IGameSelectable selectable)
    {
        selectionIndicator.SetActive(true);
        selectionIndicator.transform.position = selectable.Transform.position + offset;
    }

    private void HandleDeselected(IGameSelectable selectable)
    {
        selectionIndicator.SetActive(false);
    }

    private void OnDestroy()
    {
        SelectionSystem.OnSelected -= HandleSelected;
        SelectionSystem.OnDeselected -= HandleDeselected;
    }
}
