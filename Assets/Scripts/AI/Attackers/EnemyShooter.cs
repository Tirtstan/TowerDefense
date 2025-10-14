using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public sealed class EnemyShooter : EnemyAttack
{
    public override event Action OnAttack;

    [Header("Enemy")]
    [SerializeField]
    private EnemySO enemySO;

    [Header("Projectile")]
    [SerializeField]
    private Projectile projectilePrefab;

    [SerializeField]
    private Transform shootPoint;

    [SerializeField]
    private Aimer aimer;

    private const int DefaultCapacity = 2;
    private const int MaxSize = 5;

    private ObjectPool<Projectile> projectilePool;
    private Transform closestTarget;

    private void Awake()
    {
        if (shootPoint == null)
            shootPoint = transform;

        projectilePool = new ObjectPool<Projectile>(
            createFunc: () => Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation, transform),
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
        if (aimer == null)
            return;

        aimer.AimAt(closestTarget);
    }

    private void ShootProjectile(Transform target)
    {
        Projectile projectile = projectilePool.Get();
        projectile.transform.SetPositionAndRotation(shootPoint.position, shootPoint.rotation);
        projectile.Initialize(enemySO.Damage, target, projectilePool);
    }

    private Transform GetClosestTarget(IEnumerable<IDamagable> targets)
    {
        Transform closest = null;
        float minSqrDistance = float.MaxValue;

        foreach (var target in targets)
        {
            if (target.Target == null)
                continue;

            float sqrDistance = (transform.position - target.Target.position).sqrMagnitude;
            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;
                closest = target.Target;
            }
        }

        return closest;
    }
}
