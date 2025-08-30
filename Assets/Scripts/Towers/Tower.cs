using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private TowerSO towerSO;

    public TowerSO GetTowerSO() => towerSO;
}
