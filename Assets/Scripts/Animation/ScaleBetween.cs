using DG.Tweening;
using UnityEngine;

public class ScaleBetween : MonoBehaviour
{
    [Header("Configs")]
    [SerializeField]
    private Vector3 from = Vector3.one;

    [SerializeField]
    private Vector3 to = Vector3.one * 1.2f;

    [SerializeField]
    [Range(0, 2)]
    private float duration = 0.5f;

    [SerializeField]
    private Ease ease = Ease.InOutSine;

    [SerializeField]
    private bool loop = true;
    private Tween tween;

    private void OnEnable()
    {
        tween = transform.DOScale(to, duration).SetEase(ease).SetLoops(loop ? -1 : 0, LoopType.Yoyo).From(from);
    }

    private void OnDisable()
    {
        tween?.Kill();
    }
}
