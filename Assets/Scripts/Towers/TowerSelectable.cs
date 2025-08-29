using UnityEngine;

[RequireComponent(typeof(Tower))]
public class TowerSelectable : MonoBehaviour, IGameSelectable
{
    [Header("Components")]
    [SerializeField]
    private RectTransform rangeElement;

    [Header("Configs")]
    [SerializeField]
    private bool alwaysShowRangeIndicator;
    private Tower tower;
    private TowerStats towerStats;

    private void Awake()
    {
        tower = GetComponent<Tower>();
        towerStats = tower.GetTowerSO().Stats;

        ToggleRangeIndicator(alwaysShowRangeIndicator);
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

    private void ToggleRangeIndicator(bool isActive)
    {
        if (isActive)
            rangeElement.sizeDelta = towerStats.Range * 2 * Vector2.one;

        rangeElement.gameObject.SetActive(isActive);
    }
}
