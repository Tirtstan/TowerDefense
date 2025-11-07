using System.Collections.Generic;
using UnityEngine;

public class TowerDirector : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private TowerAttack towerAttack;

    [SerializeField]
    private Tower tower;

    private TowerStats effectiveStats;
    private const int MaxHitColliders = 50;
    private readonly Collider[] hitColliders = new Collider[MaxHitColliders];
    private readonly List<IDamagable> targets = new();
    private float currentTime;

    private void Awake()
    {
        UpdateEffectiveStats();
        tower.OnUpgraded += OnTowerUpgraded;
    }

    private void Start()
    {
        AttackAllTargetsInRange();
    }

    private void OnTowerUpgraded(Tower upgradedTower) => UpdateEffectiveStats();

    private void UpdateEffectiveStats() => effectiveStats = tower.GetEffectiveStats();

    private void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= effectiveStats.AttackInterval)
        {
            currentTime = 0;
            AttackAllTargetsInRange();
        }
    }

    public void AttackAllTargetsInRange()
    {
        targets.Clear();
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            effectiveStats.Range,
            hitColliders,
            tower.GetTowerSO().EnemyLayer
        );

        for (int i = 0; i < hitCount; i++)
        {
            if (hitColliders[i].TryGetComponent(out IDamagable damagable))
                targets.Add(damagable);
        }

        if (targets.Count > 0)
            towerAttack.Attack(targets);
    }

    private void OnDestroy()
    {
        tower.OnUpgraded -= OnTowerUpgraded;
    }

    private void Reset()
    {
        if (tower == null)
            tower = GetComponent<Tower>();

        if (towerAttack == null)
            towerAttack = GetComponent<TowerAttack>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, tower.GetEffectiveStats().Range);
    }
}
