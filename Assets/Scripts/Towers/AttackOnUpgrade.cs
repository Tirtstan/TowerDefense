using UnityEngine;

public class AttackOnUpgrade : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Tower tower;
    private TowerDirector towerDirector;

    private void Awake()
    {
        tower.OnUpgraded += OnTowerUpgraded;
        towerDirector = tower.GetComponent<TowerDirector>();
    }

    private void OnTowerUpgraded(Tower tower)
    {
        if (towerDirector != null)
            towerDirector.AttackAllTargetsInRange();
    }

    private void OnDestroy()
    {
        tower.OnUpgraded -= OnTowerUpgraded;
    }
}
