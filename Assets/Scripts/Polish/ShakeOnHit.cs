using DG.Tweening;
using UnityEngine;

public class ShakeOnHit : EffectOnHit
{
    [Header("Configs")]
    [SerializeField]
    private float duration = 0.2f;

    [SerializeField]
    private float strength = 0.1f;

    [SerializeField]
    private int vibrato = 10;

    [SerializeField]
    private float randomness = 90f;
    private Tween tween;

    protected override void OnHit(IDamagable damagable)
    {
        tween?.Kill();
        tween = transform.DOShakePosition(duration, strength, vibrato, randomness).SetLink(gameObject);
    }
}
