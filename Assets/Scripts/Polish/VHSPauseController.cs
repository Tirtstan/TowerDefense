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
    private bool isPaused;
    private bool isGameOver;

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
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnPauseToggle(bool isPaused)
    {
        this.isPaused = isPaused;
        UpdateEffect();
    }

    private void OnGameStateChanged(GameState state)
    {
        isGameOver = state == GameState.GameOver;
        UpdateEffect();
    }

    private void UpdateEffect()
    {
        float targetIntensity = (isPaused || isGameOver) ? 1f : 0f;
        AnimateEffect(targetIntensity);
    }

    private void AnimateEffect(float targetIntensity)
    {
        fadeMotion.TryCancel();
        fadeMotion = LMotion
            .Create(vhsEffect.intensity.value, targetIntensity, fadeDuration)
            .WithEase(Ease.OutCubic)
            .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
            .Bind(vhsEffect, (value, vhsEffect) => vhsEffect.intensity.value = value)
            .AddTo(gameObject);
    }

    private void OnDestroy()
    {
        PauseManager.OnPauseToggle -= OnPauseToggle;
        GameManager.OnGameStateChanged -= OnGameStateChanged;
        fadeMotion.TryCancel();
    }
}
