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
    private bool hasValidTarget;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        visionCollider = GetComponent<SphereCollider>();

        if (enemyAttacker == null)
            enemyAttacker = GetComponent<EnemyAttacker>();

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
        if (!hasValidTarget || currentTargetDamagable == null || enemyAttacker == null)
            return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= enemySO.AttackInterval)
        {
            attackTimer = 0f;
            TryAttackCurrentTarget();
        }
    }

    private void TryAttackCurrentTarget()
    {
        if (!hasValidTarget || currentTargetDamagable == null)
            return;

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
                Debug.Log($"Enemy {gameObject.name} detected tower: {tower.name}");

                // only update when a new tower is detected
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
            {
                // only update when losing sight of current target
                UpdateTargetingOnTowerLost(tower);
                Debug.Log($"Enemy {gameObject.name} lost sight of tower: {tower.name}");
            }
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
            float currentDistance = Vector3.Distance(transform.position, currentTarget.position);
            float newDistance = Vector3.Distance(transform.position, tower.position);
            return newDistance < currentDistance;
        }

        return true;
    }

    private void FindAndSetTarget()
    {
        // Clean up any null references first
        detectedTowers.RemoveAll(tower => tower == null);

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

        return closestTower;
    }

    private void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogWarning($"Enemy {gameObject.name}: Trying to set null target!");
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
        Debug.Log($"Enemy {gameObject.name} switched target to: {currentTarget.name}");
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
        Debug.Log($"Enemy {gameObject.name}: Current target died, finding new target");
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

    private void OnDrawGizmosSelected()
    {
        if (enemySO == null)
            return;

        // Vision range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemySO.VisionRange);

        // Attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Line to current target
        if (hasValidTarget && currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}
