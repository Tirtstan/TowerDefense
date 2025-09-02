using UnityEngine;
using UnityEngine.Pool;

public abstract class Projectile : MonoBehaviour, IProjectile
{
    [Header("Projectile")]
    [SerializeField]
    protected ProjectileSO projectileSO;

    protected ObjectPool<Projectile> pool;
    protected float lifetime;

    public virtual void Initialize(float damage, Transform target, ObjectPool<Projectile> pool)
    {
        this.pool = pool;
        lifetime = 0f;
        InitializeProjectile(damage, target);
    }

    protected abstract void InitializeProjectile(float damage, Transform target);

    protected virtual void FixedUpdate()
    {
        lifetime += Time.fixedDeltaTime;

        if (lifetime >= projectileSO.Lifetime)
        {
            ReleaseToPool();
            return;
        }

        UpdateProjectile();
    }

    protected abstract void UpdateProjectile();

    protected virtual void ReleaseToPool()
    {
        if (pool != null)
        {
            transform.SetPositionAndRotation(transform.parent.position, Quaternion.identity);
            pool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
