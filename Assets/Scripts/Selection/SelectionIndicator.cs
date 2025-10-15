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

    private void HandleSelected(IGameSelectable selectable) => ShowSelector(selectable);

    private void HandleDeselected(IGameSelectable selectable) => HideSelector();

    public void ShowSelector(IGameSelectable selectable)
    {
        if (selectable == null || selectable.Transform == null)
            return;

        selectionIndicator.SetActive(true);
        selectionIndicator.transform.position = selectable.Transform.position + offset;
    }

    public void HideSelector() => selectionIndicator.SetActive(false);

    private void OnDestroy()
    {
        SelectionSystem.OnSelected -= HandleSelected;
        SelectionSystem.OnDeselected -= HandleDeselected;
    }
}
