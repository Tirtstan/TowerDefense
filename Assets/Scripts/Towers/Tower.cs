using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Tower")]
    [SerializeField]
    private TowerSO towerSO;

    public TowerSO GetTowerSO() => towerSO;
}
