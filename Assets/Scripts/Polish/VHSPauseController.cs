using LitMotion;
using UnityEngine;
using UnityEngine.Rendering;

public class VHSPauseController : MonoBehaviour
{
    [Header("Volume")]
    [SerializeField]
    private Volume volume;

    [Header("Transition")]
    [SerializeField]
    private float fadeDuration = 0.3f;

    private VHSPauseEffect vhsEffect;
    private MotionHandle fadeMotion;

    private void Awake()
    {
        if (volume == null)
        {
            Debug.LogError("Volume reference is missing on VHSPauseController!");
            enabled = false;
            return;
        }

        if (!volume.profile.TryGet(out vhsEffect))
        {
            Debug.LogError("VHSPauseEffect not found in Volume profile! Add it to the Volume.");
            enabled = false;
            return;
        }

        PauseManager.OnPauseToggle += OnPauseToggle;
    }

    private void OnPauseToggle(bool isPaused) => AnimateEffect(isPaused ? 1f : 0f);

    private void AnimateEffect(float targetIntensity)
    {
        fadeMotion.TryCancel();
        fadeMotion = LMotion
            .Create(vhsEffect.intensity.value, targetIntensity, fadeDuration)
            .WithEase(Ease.OutCubic)
            .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
            .Bind(vhsEffect.intensity, (value, parameter) => parameter.value = value)
            .AddTo(gameObject);
    }

    private void OnDestroy()
    {
        PauseManager.OnPauseToggle -= OnPauseToggle;
        fadeMotion.TryCancel();
    }
}
