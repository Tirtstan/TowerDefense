using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(SphereCollider))]
public class EnemyController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Enemy enemy;

    [SerializeField]
    private NavMeshAgent agent;

    [SerializeField]
    private EnemyAttack enemyAttack;

    [Header("Configs")]
    [SerializeField]
    private LayerMask towerLayer;

    private SphereCollider visionCollider;
    private Transform currentTarget;
    private IDamagable currentTargetDamagable;
    private readonly List<Transform> detectedTowers = new();
    private float attackTimer;
    private bool hasValidTarget;
    private bool pendingDestinationSet;
    private EnemyStats effectiveStats;

    private void Awake()
    {
        visionCollider = GetComponent<SphereCollider>();
        if (enemy == null)
            enemy = GetComponent<Enemy>();

        if (enemy != null)
        {
            UpdateEffectiveStats();
            enemy.OnMutationApplied += OnMutationApplied;
        }

        SetupComponents();
    }

    private void OnMutationApplied(Enemy mutatedEnemy) => UpdateEffectiveStats();

    private void UpdateEffectiveStats()
    {
        if (enemy != null)
            effectiveStats = enemy.GetEffectiveStats();
    }

    private void SetupComponents()
    {
        if (enemy != null && agent != null)
        {
            effectiveStats = enemy.GetEffectiveStats();
            agent.speed = effectiveStats.Speed;
            agent.stoppingDistance = effectiveStats.AttackRange;
            visionCollider.radius = effectiveStats.VisionRange;
        }

        visionCollider.isTrigger = true;
    }

    private void OnEnable()
    {
        attackTimer = 0f;
        FindAndSetTarget();
        pendingDestinationSet = true;
    }

    private void Update()
    {
        HandleAttacking();
        HandleRotation();
        TryApplyPendingDestination();
    }

    private void HandleRotation()
    {
        if (!hasValidTarget || currentTarget == null)
            return;

        if (!agent.enabled || !agent.isOnNavMesh)
            return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                Time.deltaTime * agent.angularSpeed
            );
        }
    }

    private void HandleAttacking()
    {
        if (!hasValidTarget || currentTargetDamagable == null || enemyAttack == null)
            return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= effectiveStats.AttackInterval)
        {
            attackTimer = 0f;
            TryAttackCurrentTarget();
        }
    }

    private bool IsTargetValid()
    {
        if (currentTarget == null || currentTargetDamagable == null)
            return false;

        if (currentTargetDamagable.CurrentHealth <= 0f)
            return false;

        if (!currentTarget.gameObject.activeInHierarchy)
            return false;

        return true;
    }

    private void TryAttackCurrentTarget()
    {
        if (!IsTargetValid())
        {
            FindAndSetTarget();
            return;
        }

        if (!agent.enabled || !agent.isOnNavMesh)
            return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            enemyAttack.Attack(new[] { currentTargetDamagable });
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsTower(other))
        {
            Transform tower = other.transform;
            if (!detectedTowers.Contains(tower))
            {
                detectedTowers.Add(tower);
                UpdateTargetingOnTowerDetected(tower);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsTower(other))
        {
            Transform tower = other.transform;
            bool wasRemoved = detectedTowers.Remove(tower);

            if (wasRemoved)
                UpdateTargetingOnTowerLost(tower);
        }
    }

    private void UpdateTargetingOnTowerDetected(Transform newTower)
    {
        // If we have no current target yet, immediately pick the newly detected tower
        if (!hasValidTarget || currentTarget == null)
        {
            SetTarget(newTower);
            return;
        }

        // if we're targeting center tower, immediately switch to any detected tower
        if (currentTarget == TowerManager.Instance.GetCenterTowerTransform())
        {
            SetTarget(newTower);
            return;
        }

        // if this new tower is closer than current target, switch to it
        if (hasValidTarget && ShouldSwitchToTower(newTower))
            SetTarget(newTower);
    }

    private void UpdateTargetingOnTowerLost(Transform lostTower)
    {
        if (currentTarget == lostTower)
            FindAndSetTarget();
    }

    private bool IsTower(Collider collider) => ((1 << collider.gameObject.layer) & towerLayer) != 0;

    private bool ShouldSwitchToTower(Transform tower)
    {
        // always switch if targeting center tower
        if (currentTarget == TowerManager.Instance.GetCenterTowerTransform())
            return true;

        // switch if this tower is closer than current target
        if (hasValidTarget && currentTarget != null)
        {
            float currentSqrDistance = (transform.position - currentTarget.position).sqrMagnitude;
            float newSqrDistance = (transform.position - tower.position).sqrMagnitude;
            return newSqrDistance < currentSqrDistance;
        }

        return true;
    }

    private void FindAndSetTarget()
    {
        // Clean up any null references first
        detectedTowers.RemoveAll(tower => tower == null || !tower.gameObject.activeInHierarchy);

        Transform bestTarget = GetClosestDetectedTower();

        // If no detected towers, default to center tower
        if (bestTarget == null && TowerManager.Instance.GetCenterTowerTransform() != null)
            bestTarget = TowerManager.Instance.GetCenterTowerTransform();

        SetTarget(bestTarget);
    }

    private Transform GetClosestDetectedTower()
    {
        if (detectedTowers.Count == 0)
            return null;

        Transform closestTower = null;
        float closestSqrDistance = float.MaxValue;

        foreach (var tower in detectedTowers)
        {
            if (tower == null || !tower.gameObject.activeInHierarchy)
                continue;

            if (!tower.TryGetComponent(out IDamagable damagable) || damagable.CurrentHealth <= 0f)
                continue;

            float sqrDistance = (transform.position - tower.position).sqrMagnitude;
            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closestTower = tower;
            }
        }

        return closestTower;
    }

    private void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
        {
            hasValidTarget = false;
            if (agent != null && agent.enabled)
                agent.isStopped = true;
            return;
        }

        UnsubscribeFromTargetDeath(currentTargetDamagable);

        currentTarget = newTarget;
        hasValidTarget = true;

        // Defer destination until agent is enabled and placed on NavMesh to avoid warnings
        pendingDestinationSet = true;
        if (agent != null && agent.enabled)
            agent.isStopped = false;

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

    private void TryApplyPendingDestination()
    {
        if (!pendingDestinationSet || !hasValidTarget || currentTarget == null || agent == null)
            return;

        if (!agent.enabled || !agent.isOnNavMesh)
            return; // Wait until spawner/engine enables agent and places it on the NavMesh

        agent.isStopped = false;
        agent.SetDestination(currentTarget.position);
        pendingDestinationSet = false;
    }

    private void SubscribeToTargetDeath(IDamagable damagable)
    {
        if (damagable != null)
            damagable.OnDeath += OnTargetDeath;
    }

    private void UnsubscribeFromTargetDeath(IDamagable damagable)
    {
        if (damagable != null)
            damagable.OnDeath -= OnTargetDeath;
    }

    private void OnTargetDeath()
    {
        UnsubscribeFromTargetDeath(currentTargetDamagable);
        ResetController();

        // find new target when current one dies
        FindAndSetTarget();
    }

    private void OnDisable()
    {
        UnsubscribeFromTargetDeath(currentTargetDamagable);
        ResetController();
    }

    private void ResetController()
    {
        currentTarget = null;
        currentTargetDamagable = null;
        detectedTowers.Clear();
        hasValidTarget = false;
        attackTimer = 0f;
    }

    private void OnValidate()
    {
        visionCollider = GetComponent<SphereCollider>();
        SetupComponents();
    }

    private void OnDrawGizmosSelected()
    {
        if (enemy == null)
            return;

        EnemyStats stats = enemy.GetEffectiveStats();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.VisionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stats.AttackRange);

        if (hasValidTarget && currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }

    private void OnDestroy()
    {
        if (enemy != null)
            enemy.OnMutationApplied -= OnMutationApplied;
    }
}
