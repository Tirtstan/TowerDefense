using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(IAttack))]
public class KnockbackVisualOnAttack : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Transform visual;

    [Header("Knockback Settings")]
    [SerializeField]
    private float knockbackDistance = 0.2f;

    [SerializeField]
    private float knockbackDuration = 0.1f;

    [SerializeField]
    private float returnDuration = 0.25f;

    [SerializeField]
    private Ease startEaseType = Ease.OutSine;

    [SerializeField]
    private Ease returnEaseType = Ease.OutElastic;
    private IAttack attacker;
    private Vector3 originalPosition;
    private Sequence knockbackSequence;

    private void Awake()
    {
        attacker = GetComponent<IAttack>();
        attacker.OnAttack += HandleOnAttack;

        originalPosition = visual.localPosition;
    }

    private void HandleOnAttack()
    {
        if (visual == null)
            return;

        knockbackSequence?.Kill();
        knockbackSequence = DOTween.Sequence();

        knockbackSequence
            .Append(
                visual
                    .DOLocalMove(originalPosition + Vector3.back * knockbackDistance, knockbackDuration)
                    .SetEase(startEaseType)
            )
            .Append(visual.DOLocalMove(originalPosition, returnDuration).SetEase(returnEaseType));
    }

    private void OnDestroy()
    {
        attacker.OnAttack -= HandleOnAttack;
        knockbackSequence?.Kill();
    }
}
