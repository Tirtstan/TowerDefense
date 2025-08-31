using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(SphereCollider))]
public class EnemyController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private EnemySO enemySO;

    [SerializeField]
    private EnemyAttacker enemyAttacker;

    [Header("Configs")]
    [SerializeField]
    private LayerMask towerLayer;

    [SerializeField]
    private float attackRange = 2f;
    private NavMeshAgent agent;
    private SphereCollider visionCollider;
    private Transform currentTarget;
    private IDamagable currentTargetDamagable;
    private readonly List<Transform> detectedTowers = new();
    private float attackTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = enemySO.Speed;

        visionCollider = GetComponent<SphereCollider>();
        visionCollider.isTrigger = true;
        visionCollider.radius = enemySO.VisionRange;

        if (enemyAttacker == null)
            enemyAttacker = GetComponent<EnemyAttacker>();
    }

    private void Start()
    {
        currentTarget = CenterTower.Instance.transform;
        agent.SetDestination(currentTarget.position);

        if (currentTarget.TryGetComponent(out IDamagable damagable))
        {
            currentTargetDamagable = damagable;
            SubscribeToTargetDeath(damagable);
        }
    }

    private void Update()
    {
        if (currentTarget != null && currentTargetDamagable != null)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= enemySO.AttackInterval)
            {
                attackTimer = 0f;
                TryAttackCurrentTarget();
            }
        }

        HandleTargetSwitching();
    }

    private void HandleTargetSwitching()
    {
        // if current target is still valid
        if (currentTarget != null && currentTarget != CenterTower.Instance.transform)
        {
            if (!detectedTowers.Contains(currentTarget))
            {
                // current target is no longer detected, switch back to center or find new target
                SwitchToClosestTower();
            }
        }
        else if (detectedTowers.Count > 0 && currentTarget == CenterTower.Instance.transform)
        {
            // targeting center but have detected towers, switch to closest
            SwitchToClosestTower();
        }
    }

    private void TryAttackCurrentTarget()
    {
        // Double check target existence
        if (currentTarget == null || currentTargetDamagable == null)
        {
            SwitchToClosestTower();
            return;
        }

        // Check if we're close enough to attack
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        if (distanceToTarget <= attackRange)
            enemyAttacker.Attack(new[] { currentTargetDamagable });
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsTower(other))
        {
            Transform tower = other.transform;
            if (!detectedTowers.Contains(tower))
            {
                detectedTowers.Add(tower);

                // If we're currently targeting the center tower, switch to this closer one
                if (currentTarget == CenterTower.Instance.transform)
                    SwitchTarget(tower);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsTower(other))
        {
            Transform tower = other.transform;
            detectedTowers.Remove(tower);

            // If this was our current target, find a new one
            if (currentTarget == tower)
                SwitchToClosestTower();
        }
    }

    private bool IsTower(Collider collider) => ((1 << collider.gameObject.layer) & towerLayer) != 0;

    private void SwitchToClosestTower()
    {
        if (detectedTowers.Count == 0)
        {
            SwitchTarget(CenterTower.Instance.transform);
            return;
        }

        if (detectedTowers.Count == 1)
        {
            SwitchTarget(detectedTowers[0]);
            return;
        }

        Transform closestTower = null;
        float closestDistance = float.MaxValue;

        foreach (var tower in detectedTowers)
        {
            if (tower == null)
                continue;

            float distance = Vector3.Distance(transform.position, tower.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTower = tower;
            }
        }

        if (closestTower != null)
            SwitchTarget(closestTower);
    }

    private void SwitchTarget(Transform newTarget)
    {
        if (currentTargetDamagable != null)
            UnsubscribeFromTargetDeath(currentTargetDamagable);

        currentTarget = newTarget;
        agent.SetDestination(currentTarget.position);

        if (currentTarget.TryGetComponent(out IDamagable damagable))
        {
            currentTargetDamagable = damagable;
            SubscribeToTargetDeath(damagable);
        }
        else
        {
            currentTargetDamagable = null;
        }

        attackTimer = 0f;
    }

    private void SubscribeToTargetDeath(IDamagable damagable)
    {
        damagable.OnDeath += OnTargetDeath;
    }

    private void UnsubscribeFromTargetDeath(IDamagable damagable)
    {
        damagable.OnDeath -= OnTargetDeath;
    }

    private void OnTargetDeath()
    {
        currentTargetDamagable = null;
        SwitchToClosestTower();
    }

    private void OnDestroy()
    {
        if (currentTargetDamagable != null)
            UnsubscribeFromTargetDeath(currentTargetDamagable);
    }

    private void OnDrawGizmosSelected()
    {
        // vision range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, enemySO.VisionRange);

        // attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
