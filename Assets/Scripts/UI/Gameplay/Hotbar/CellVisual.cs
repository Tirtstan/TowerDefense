using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CellVisual : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private RectTransform shakeTransform;

    [SerializeField]
    private Image fillImage;

    [SerializeField]
    private TextMeshProUGUI costText;

    [SerializeField]
    private TowerSO towerSO;

    [Header("Text Colour")]
    [SerializeField]
    private Color costReachedColor;

    [Header("Fill Animation")]
    [SerializeField]
    private float fillDuration = 0.5f;

    [SerializeField]
    private Ease fillEase = Ease.OutCubic;

    [Header("Shake")]
    [SerializeField]
    private float shakeDuration = 0.2f;

    [SerializeField]
    private float shakeStrength = 0.1f;
    private Tween tween;
    private bool canRepeatAnim = true;
    private Color defaultTextColor;

    private void Awake()
    {
        fillImage.fillAmount = 0;
        defaultTextColor = costText.color;

        EconomyManager.OnCurrencyUpdated += OnCurrencyUpdated;
    }

    private void OnCurrencyUpdated(int amount)
    {
        tween?.Kill();

        float target = Mathf.Clamp01(amount / towerSO.Stats.Cost);
        tween = DOVirtual
            .Float(fillImage.fillAmount, target, fillDuration, value => fillImage.fillAmount = value)
            .SetEase(fillEase)
            .SetUpdate(UpdateType.Fixed);

        if (amount >= towerSO.Stats.Cost)
        {
            fillImage.fillAmount = 1;
            if (canRepeatAnim)
                OnCostReached();
        }
        else
        {
            costText.color = defaultTextColor;
            canRepeatAnim = true;
        }
    }

    private void OnCostReached()
    {
        canRepeatAnim = false;
        shakeTransform.DOKill();
        shakeTransform
            .DOPunchScale(Vector3.one * shakeStrength, shakeDuration)
            .SetUpdate(UpdateType.Fixed)
            .OnComplete(() => shakeTransform.localScale = Vector3.one);

        costText.color = costReachedColor;
    }

    private void OnDestroy()
    {
        EconomyManager.OnCurrencyUpdated -= OnCurrencyUpdated;
    }
}
