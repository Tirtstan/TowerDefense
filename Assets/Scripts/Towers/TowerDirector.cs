using System.Collections.Generic;
using UnityEngine;

public class TowerDirector : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private TowerAttack towerAttack;

    [SerializeField]
    private Tower tower;

    private TowerSO towerSO;
    private const int MaxHitColliders = 50;
    private readonly Collider[] hitColliders = new Collider[MaxHitColliders];
    private readonly List<IDamagable> targets = new();
    private float currentTime;

    private void Awake()
    {
        towerSO = tower.GetTowerSO();
    }

    private void Start()
    {
        AttackAllTargetsInRange();
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= towerSO.Stats.AttackInterval)
        {
            currentTime = 0;
            AttackAllTargetsInRange();
        }
    }

    private void AttackAllTargetsInRange()
    {
        targets.Clear();
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            towerSO.Stats.Range,
            hitColliders,
            towerSO.EnemyLayer
        );

        for (int i = 0; i < hitCount; i++)
        {
            if (hitColliders[i].TryGetComponent(out IDamagable damagable))
                targets.Add(damagable);
        }

        if (targets.Count > 0)
            towerAttack.Attack(targets);
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
        Gizmos.DrawWireSphere(transform.position, tower.GetTowerSO().Stats.Range);
    }
}
