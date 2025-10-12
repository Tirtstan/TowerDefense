using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public sealed class ArrowProjectile : Projectile
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    protected override void UpdateProjectile()
    {
        if (Target != null)
            MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (Target.position - transform.position).normalized;
        Vector3 newPosition = rb.position + projectileSO.Speed * Time.fixedDeltaTime * direction;

        newPosition.y = Mathf.Max(newPosition.y, projectileSO.MinYPosition);

        rb.MovePosition(newPosition);
        rb.MoveRotation(Quaternion.LookRotation(direction));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (projectileSO.HitLayers == (projectileSO.HitLayers | (1 << other.gameObject.layer)))
        {
            if (other.TryGetComponent(out IDamagable damagable))
            {
                damagable.TakeDamage(Damage);
                ReleaseToPool();
            }
        }
    }

    protected override void ReleaseToPool()
    {
        Target = null;
        base.ReleaseToPool();
    }
}
