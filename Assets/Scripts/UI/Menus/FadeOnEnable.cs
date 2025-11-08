using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeOnEnable : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField]
    [Range(0f, 1f)]
    private float fadeDuration = 0.3f;
    private CanvasGroup canvasGroup;
    private MotionHandle motionHandle;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        motionHandle.TryCancel();
        motionHandle = LMotion
            .Create(0, 1f, fadeDuration)
            .WithEase(Ease.Linear)
            .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
            .BindToAlpha(canvasGroup);
    }

    private void OnDisable()
    {
        motionHandle.TryCancel();
        canvasGroup.alpha = 0f;
    }
}
