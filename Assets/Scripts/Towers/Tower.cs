using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Tower")]
    [SerializeField]
    private TowerSO towerSO;

    public TowerSO GetTowerSO() => towerSO;

    private void Start()
    {
        TowerManager.Instance.RegisterTower(this);
    }

    private void OnDestroy()
    {
        if (TowerManager.Instance != null)
            TowerManager.Instance.UnregisterTower(this);
    }
}
