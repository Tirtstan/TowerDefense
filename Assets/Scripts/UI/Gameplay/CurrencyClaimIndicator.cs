using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CurrencyClaimIndicator : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private GenerateCurrency generateCurrency;

    [Header("Animation Settings")]
    [SerializeField]
    [Range(0.1f, 2f)]
    [Tooltip("Duration of one pulse cycle")]
    private float pulseDuration = 0.5f;

    [SerializeField]
    [Range(1f, 2f)]
    [Tooltip("Scale multiplier when pulsing")]
    private float scaleMultiplier = 1.2f;

    [SerializeField]
    [Tooltip("Color when currency is ready to claim")]
    private Color claimReadyColor = Color.green;

    [SerializeField]
    private Ease scaleEase = Ease.InOutSine;

    [SerializeField]
    private Ease colorEase = Ease.Linear;

    private TextMeshProUGUI text;
    private Color originalColor;
    private Vector3 originalScale;
    private MotionHandle scaleMotionHandle;
    private MotionHandle colorMotionHandle;
    private bool isAnimating;

    private void Awake()
    {
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();

        if (generateCurrency == null)
            generateCurrency = GetComponentInParent<GenerateCurrency>();

        originalColor = text.color;
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (generateCurrency != null)
        {
            generateCurrency.OnCurrencyAmountChanged += OnCurrencyAmountChanged;
            generateCurrency.OnCurrencyClaimed += OnCurrencyClaimed;
            generateCurrency.OnCurrencyExpired += OnCurrencyExpired;
        }
    }

    private void OnDisable()
    {
        if (generateCurrency != null)
        {
            generateCurrency.OnCurrencyAmountChanged -= OnCurrencyAmountChanged;
            generateCurrency.OnCurrencyClaimed -= OnCurrencyClaimed;
            generateCurrency.OnCurrencyExpired -= OnCurrencyExpired;
        }

        StopAnimation();
    }

    private void OnCurrencyAmountChanged(int amount, float percentage)
    {
        if (generateCurrency.HasCurrencyToClaim && !isAnimating)
        {
            StartPulseAnimation();
        }
        else if (!generateCurrency.HasCurrencyToClaim && isAnimating)
        {
            StopAnimation();
        }
    }

    private void OnCurrencyClaimed(int amount, float percentage)
    {
        StopAnimation();
    }

    private void OnCurrencyExpired(float percentage)
    {
        StopAnimation();
    }

    private void StartPulseAnimation()
    {
        if (isAnimating || !gameObject.activeInHierarchy)
            return;

        isAnimating = true;

        // Lerp color in once
        colorMotionHandle.TryCancel();
        colorMotionHandle = LMotion
            .Create(text.color, claimReadyColor, pulseDuration)
            .WithEase(colorEase)
            .BindToColor(text)
            .AddTo(gameObject);

        PulseLoop();
    }

    private void PulseLoop()
    {
        if (!isAnimating || !gameObject.activeInHierarchy)
            return;

        // Scale animation only (color stays at claimReadyColor)
        scaleMotionHandle.TryCancel();
        scaleMotionHandle = LMotion
            .Create(originalScale, originalScale * scaleMultiplier, pulseDuration)
            .WithEase(scaleEase)
            .WithOnComplete(() =>
            {
                if (isAnimating && gameObject.activeInHierarchy)
                {
                    scaleMotionHandle = LMotion
                        .Create(transform.localScale, originalScale, pulseDuration)
                        .WithEase(scaleEase)
                        .WithOnComplete(() =>
                        {
                            if (isAnimating && generateCurrency != null && generateCurrency.HasCurrencyToClaim)
                                PulseLoop();
                        })
                        .BindToLocalScale(transform)
                        .AddTo(gameObject);
                }
            })
            .BindToLocalScale(transform)
            .AddTo(gameObject);
    }

    private void StopAnimation()
    {
        isAnimating = false;
        scaleMotionHandle.TryCancel();
        colorMotionHandle.TryCancel();

        if (gameObject.activeInHierarchy)
        {
            // return to original state
            scaleMotionHandle = LMotion
                .Create(transform.localScale, originalScale, pulseDuration * 0.5f)
                .WithEase(Ease.OutSine)
                .BindToLocalScale(transform)
                .AddTo(gameObject);

            colorMotionHandle = LMotion
                .Create(text.color, originalColor, pulseDuration * 0.5f)
                .WithEase(Ease.OutSine)
                .BindToColor(text)
                .AddTo(gameObject);
        }
        else
        {
            // If inactive, reset immediately
            transform.localScale = originalScale;
            text.color = originalColor;
        }
    }

    private void Reset()
    {
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();

        if (generateCurrency == null)
            generateCurrency = GetComponentInParent<GenerateCurrency>();
    }
}
