using System.Collections.Generic;
using UnityEngine;

public class TowerDirector : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private TowerAttacker towerAttack;

    [SerializeField]
    private TowerSO towerSO;

    [SerializeField]
    private LayerMask enemyLayerMask = -1;

    private const int MaxHitColliders = 50;
    private readonly Collider[] hitColliders = new Collider[MaxHitColliders];
    private readonly List<IDamagable> targets = new();
    private float currentTime;

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
            enemyLayerMask
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
        if (towerAttack == null)
            towerAttack = GetComponent<TowerAttacker>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, towerSO.Stats.Range);
    }
}
