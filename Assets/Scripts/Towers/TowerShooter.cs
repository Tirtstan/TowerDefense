using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class TowerShooter : TowerAttack
{
    public override event Action OnAttack;

    [Header("Components")]
    [SerializeField]
    private Tower tower;

    [SerializeField]
    private Projectile projectilePrefab;

    [SerializeField]
    private Transform shootPoint;

    [Header("Visuals")]
    [SerializeField]
    private Transform visual;

    private const int DefaultCapacity = 2;
    private const int MaxSize = 5;
    private const float TurnRate = 10f;

    private ObjectPool<Projectile> projectilePool;
    private Transform closestTarget;

    private void Awake()
    {
        if (shootPoint == null)
            shootPoint = transform;

        projectilePool = new ObjectPool<Projectile>(
            createFunc: () => Instantiate(projectilePrefab, shootPoint),
            actionOnGet: (projectile) => projectile.gameObject.SetActive(true),
            actionOnRelease: (projectile) => projectile.gameObject.SetActive(false),
            actionOnDestroy: (projectile) => Destroy(projectile.gameObject),
            collectionCheck: true,
            DefaultCapacity,
            MaxSize
        );
    }

    public override void Attack(IEnumerable<IDamagable> targets)
    {
        closestTarget = GetClosestTarget(targets);
        if (closestTarget != null)
        {
            ShootProjectile(closestTarget);
            OnAttack?.Invoke();
        }
    }

    private void Update()
    {
        if (visual == null || closestTarget == null)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(closestTarget.position - visual.position);
        visual.rotation = Quaternion.Slerp(visual.rotation, targetRotation, Time.deltaTime * TurnRate);
    }

    private void ShootProjectile(Transform closestTarget)
    {
        Projectile projectile = projectilePool.Get();
        projectile.transform.SetPositionAndRotation(shootPoint.position, Quaternion.identity);
        projectile.Initialize(tower.GetTowerSO().Stats.Damage, closestTarget, projectilePool);
    }

    private Transform GetClosestTarget(IEnumerable<IDamagable> targets)
    {
        Transform closest = null;
        float minSqrDistance = float.MaxValue;

        foreach (var target in targets)
        {
            float sqrDistance = (transform.position - target.Target.position).sqrMagnitude;
            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;
                closest = target.Target;
            }
        }

        return closest;
    }

    private void Reset()
    {
        if (tower == null)
            tower = GetComponent<Tower>();
    }
}
