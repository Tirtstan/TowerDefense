using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(SphereCollider))]
public class EnemyController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private EnemySO enemySO;

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

    private void Awake()
    {
        visionCollider = GetComponent<SphereCollider>();
        SetupComponents();
    }

    private void SetupComponents()
    {
        if (enemySO != null)
        {
            agent.speed = enemySO.Speed;
            visionCollider.radius = enemySO.VisionRange;
        }

        visionCollider.isTrigger = true;
    }

    private void Start()
    {
        FindAndSetTarget();
    }

    private void Update()
    {
        HandleAttacking();
    }

    private void HandleAttacking()
    {
        if (!hasValidTarget || currentTargetDamagable == null || enemyAttack == null)
            return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= enemySO.AttackInterval)
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

        float sqrDistanceToTarget = (transform.position - currentTarget.position).sqrMagnitude;
        if (sqrDistanceToTarget <= enemySO.AttackRange * enemySO.AttackRange)
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
        // if we're targeting center tower, immediately switch to any detected tower
        if (currentTarget == CenterTower.Instance.transform)
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
        if (currentTarget == CenterTower.Instance.transform)
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
        if (bestTarget == null)
            bestTarget = CenterTower.Instance.transform;

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
            return;
        }

        UnsubscribeFromTargetDeath(currentTargetDamagable);

        currentTarget = newTarget;
        hasValidTarget = true;
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

        currentTargetDamagable = null;
        currentTarget = null;
        hasValidTarget = false;

        // find new target when current one dies
        FindAndSetTarget();
    }

    private void OnDestroy()
    {
        UnsubscribeFromTargetDeath(currentTargetDamagable);
    }

    private void OnValidate()
    {
        visionCollider = GetComponent<SphereCollider>();
        SetupComponents();
    }

    private void OnDrawGizmosSelected()
    {
        if (enemySO == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemySO.VisionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemySO.AttackRange);

        if (hasValidTarget && currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}
