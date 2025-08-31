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
    private Tower tower;

    private MeshFilter towerMeshFilter;
    public Transform Transform => transform;

    private void Awake()
    {
        tower = GetComponent<Tower>();
        towerMeshFilter = towerMeshRenderer.GetComponent<MeshFilter>();

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
        TowerInfoMenu.Instance.HideMenu();

        if (!alwaysShowRangeIndicator)
            ToggleRangeIndicator(false);
    }

    private void ToggleRangeIndicator(bool isActive) => rangeElement.gameObject.SetActive(isActive);

    public (MeshRenderer, MeshFilter) GetMeshComponents() => (towerMeshRenderer, towerMeshFilter);
}
