using UnityEngine;
using UnityEngine.Pool;

public interface IProjectile
{
    public void Initialize(float damage, Transform target, ObjectPool<Projectile> pool);
}
