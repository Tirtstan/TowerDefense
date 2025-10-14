using UnityEngine;
using UnityEngine.Pool;

public abstract class Projectile : MonoBehaviour, IProjectile
{
    [Header("Projectile")]
    [SerializeField]
    protected ProjectileSO projectileSO;
    protected ObjectPool<Projectile> pool;
    protected float Damage { get; set; }
    protected Transform Target { get; set; }
    protected float lifetime;
    private bool isReleased;

    public virtual void Initialize(float damage, Transform target, ObjectPool<Projectile> pool)
    {
        Damage = damage;
        Target = target;
        this.pool = pool;
        lifetime = 0f;
        isReleased = false;
    }

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
        if (isReleased)
            return;

        isReleased = true;

        if (pool != null)
        {
            Target = null;
            transform.SetPositionAndRotation(transform.parent.position, Quaternion.identity);
            pool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
