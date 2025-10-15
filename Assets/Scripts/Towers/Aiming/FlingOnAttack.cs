using DG.Tweening;
using UnityEngine;

public class FlingOnAttack : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private TowerAttack towerAttack;

    [SerializeField]
    private Transform xLauncher;

    [Header("Configs")]
    [SerializeField]
    private float flingAngle = 45f;

    [SerializeField]
    [Range(0.05f, 0.5f)]
    private float flingDuration = 0.15f;

    [SerializeField]
    private Ease ease = Ease.OutExpo;
    private Tween launcherTween;

    private void Awake()
    {
        towerAttack.OnAttack += HandleOnAttack;
    }

    private void HandleOnAttack()
    {
        launcherTween?.Kill();
        launcherTween = xLauncher
            .DOLocalRotate(flingAngle * Vector3.right, flingDuration)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(ease);
    }

    private void OnDestroy()
    {
        launcherTween?.Kill();
        towerAttack.OnAttack -= HandleOnAttack;
    }
}
