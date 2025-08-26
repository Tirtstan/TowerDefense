using UnityEngine;

[RequireComponent(typeof(Tower))]
public class TowerSelectable : MonoBehaviour, IGameSelectable
{
    private Tower tower;

    private void Awake()
    {
        tower = GetComponent<Tower>();
    }

    public void Select() => TowerInfoMenu.Instance.ShowMenu(tower);

    public void Deselect() => TowerInfoMenu.Instance.HideMenu();
}
