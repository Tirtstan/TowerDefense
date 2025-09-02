using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class ArrowProjectile : Projectile
{
    private Rigidbody rb;
    private float damage;
    private Transform target;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected override void InitializeProjectile(float damage, Transform target)
    {
        this.damage = damage;
        this.target = target;
    }

    protected override void UpdateProjectile()
    {
        if (target != null)
            MoveTowardsTarget();
        else
            ReleaseToPool();
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Vector3 newPosition = rb.position + projectileSO.Speed * Time.fixedDeltaTime * direction;

        newPosition.y = Mathf.Max(newPosition.y, projectileSO.MinYPosition);

        rb.MovePosition(newPosition);
        rb.MoveRotation(Quaternion.LookRotation(direction));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamagable damagable))
        {
            if (projectileSO.HitLayers == (projectileSO.HitLayers | (1 << other.gameObject.layer)))
            {
                damagable.TakeDamage(damage);
                ReleaseToPool();
            }
        }
    }

    protected override void ReleaseToPool()
    {
        target = null;
        base.ReleaseToPool();
    }
}
