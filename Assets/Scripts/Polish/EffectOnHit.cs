using UnityEngine;

[RequireComponent(typeof(IDamagable))]
public abstract class EffectOnHit : MonoBehaviour
{
    private IDamagable damagable;

    private void Awake()
    {
        damagable = GetComponent<IDamagable>();
        damagable.OnHealthChanged += OnHit;
    }

    protected abstract void OnHit(IDamagable damagable);

    private void OnDestroy()
    {
        if (damagable != null)
            damagable.OnHealthChanged -= OnHit;
    }
}
