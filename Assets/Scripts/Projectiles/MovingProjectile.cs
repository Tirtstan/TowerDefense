using UnityEngine;
using UnityEngine.Pool;

public enum ProjectileMovementType
{
    Straight,
    Arc
}

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public sealed class MovingProjectile : Projectile
{
    [Header("Movement")]
    [SerializeField]
    private ProjectileMovementType movementType = ProjectileMovementType.Straight;

    [Header("Arc Settings (if movement is Arc)")]
    [SerializeField]
    [Tooltip(
        "Defines the arc shape. X axis is normalized time (0-1), Y axis is height multiplier. Make sure to end the curve at (1,0)."
    )]
    private AnimationCurve arcCurve;

    [SerializeField]
    private float arcHeightMultiplier = 1f;

    private Rigidbody rb;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float journeyTime;
    private Collider[] maxColliders;
    private const int MaxColliderAmount = 10;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    public override void Initialize(float damage, Transform target, ObjectPool<Projectile> pool)
    {
        base.Initialize(damage, target, pool);
        startPosition = transform.position;
        journeyTime = 0f;

        if (Target != null)
        {
            targetPosition = Target.position;
        }
        else
        {
            targetPosition = startPosition + transform.forward * 10f;
        }
    }

    protected override void UpdateProjectile()
    {
        if ((Target == null || !Target.gameObject.activeInHierarchy) && movementType == ProjectileMovementType.Straight)
        {
            ReleaseToPool();
            return;
        }

        switch (movementType)
        {
            case ProjectileMovementType.Straight:
                MoveStraight();
                break;
            case ProjectileMovementType.Arc:
                MoveInArc();
                break;
        }
    }

    private void MoveStraight()
    {
        Vector3 direction = (Target.position - transform.position).normalized;
        Vector3 newPosition = rb.position + projectileSO.Speed * Time.fixedDeltaTime * direction;

        newPosition.y = Mathf.Max(newPosition.y, projectileSO.MinYPosition);

        rb.MovePosition(newPosition);
        rb.MoveRotation(Quaternion.LookRotation(direction));
    }

    private void MoveInArc()
    {
        journeyTime += Time.fixedDeltaTime;
        float progress = Mathf.Clamp01(journeyTime / projectileSO.Speed);

        if (Target != null)
            targetPosition = Target.position;

        Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, progress);
        currentPos.y += arcCurve.Evaluate(progress) * arcHeightMultiplier;

        rb.MovePosition(currentPos);

        if (progress >= 1f)
            ReleaseToPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (projectileSO.HitLayers == (projectileSO.HitLayers | (1 << other.gameObject.layer)))
        {
            if (other.TryGetComponent(out IDamagable damagable))
            {
                damagable.TakeDamage(Damage);
                if (projectileSO is SplashProjectileSO splashSO)
                    SplashAttack(splashSO);

                ReleaseToPool();
            }
        }
    }

    private void SplashAttack(SplashProjectileSO splashSO)
    {
        if (maxColliders == null || maxColliders.Length != MaxColliderAmount)
            maxColliders = new Collider[MaxColliderAmount];

        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            splashSO.SplashRadius,
            maxColliders,
            splashSO.HitLayers
        );
        for (int i = 0; i < count; i++)
        {
            if (maxColliders[i].TryGetComponent(out IDamagable damageable))
            {
                damageable.TakeDamage(Damage * splashSO.SplashDamageMultiplier);
                if (maxColliders[i].TryGetComponent(out Rigidbody hitRb) && !hitRb.isKinematic)
                {
                    Vector3 direction = (maxColliders[i].transform.position - transform.position).normalized;
                    hitRb.AddForce(direction * splashSO.SplashForce, ForceMode.Impulse);
                }
            }
        }
    }
}
