using UnityEngine;

[RequireComponent(typeof(Tower))]
public class TowerSelectable : MonoBehaviour, IGameSelectable
{
    [Header("Components")]
    [SerializeField]
    private MeshRenderer towerMeshRenderer;

    [SerializeField]
    private RectTransform rangeElement;

    [Header("Configs")]
    [SerializeField]
    private bool alwaysShowRangeIndicator;
    public Transform Transform => transform;
    private Tower tower;

    private void Awake()
    {
        tower = GetComponent<Tower>();

        if (alwaysShowRangeIndicator)
            ToggleRangeIndicator(true);
    }

    public void Select()
    {
        TowerInfoMenu.Instance.ShowMenu(tower);
        ToggleRangeIndicator(true);
    }

    public void Deselect()
    {
        if (TowerInfoMenu.Instance == null)
            return;

        TowerInfoMenu.Instance.TryHideMenu(tower);
        if (!alwaysShowRangeIndicator)
            ToggleRangeIndicator(false);
    }

    private void ToggleRangeIndicator(bool isActive) => rangeElement.gameObject.SetActive(isActive);

    private void OnDestroy()
    {
        if (SelectionSystem.Instance != null)
            SelectionSystem.Instance.DeselectObject(this);

        Deselect();
    }
}
