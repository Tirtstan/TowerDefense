using System.Collections;
using Flexalon;
using LitMotion;
using LitMotion.Extensions;
using LitMotion.Extensions.Flexalon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TowerSelectionWorldSpace : Singleton<TowerSelectionWorldSpace>
{
    [Header("Components")]
    [SerializeField]
    private GameObject uiContainer;

    [SerializeField]
    private Button upgradeButton;

    [SerializeField]
    private TextMeshProUGUI upgradeCostText;

    [SerializeField]
    private Image fillImage;

    [Header("Positioning")]
    [SerializeField]
    private Vector3 offset = new(0, 2f, 0);

    [Header("Visuals")]
    [SerializeField]
    private Color canAffordTextColor = Color.white;

    [SerializeField]
    private Color cannotAffordTextColor = Color.red;

    [Header("Fill Animation")]
    [SerializeField]
    private float fillDuration = 0.5f;

    [SerializeField]
    private Ease fillEase = Ease.OutCubic;

    [Header("Scale Animation")]
    [SerializeField]
    private float scaleDuration = 0.3f;

    [SerializeField]
    private Ease scaleEase = Ease.OutCubic;

    [Header("Fade Animation")]
    [SerializeField]
    private float fadeDuration = 0.3f;

    [SerializeField]
    private Ease fadeEase = Ease.OutCubic;

    private Tower currentTower;
    private MotionHandle fillMotionHandle;
    private MotionHandle scaleMotionHandle;
    private MotionHandle fadeMotionHandle;
    private FlexalonObject flexalonObject;
    private CanvasGroup canvasGroup;
    private Vector3 originalScale;
    private Coroutine scaleOutCoroutine;

    protected override void Awake()
    {
        base.Awake();

        if (uiContainer != null)
        {
            uiContainer.SetActive(false);

            // Get FlexalonObject component
            flexalonObject = uiContainer.GetComponent<FlexalonObject>();
            if (flexalonObject != null)
            {
                originalScale = flexalonObject.Scale;
            }

            // Get CanvasGroup for fade animation
            canvasGroup = uiContainer.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = uiContainer.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
        }

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);

        SelectionSystem.OnSelected += OnTowerSelected;
        SelectionSystem.OnDeselected += OnTowerDeselected;
        EconomyManager.OnCurrencyUpdated += OnCurrencyUpdated;

        fillImage.fillAmount = 0f;
    }

    private void LateUpdate()
    {
        if (currentTower == null || uiContainer == null || !uiContainer.activeSelf)
            return;

        transform.position = currentTower.transform.position + offset;
    }

    private void OnTowerSelected(IGameSelectable selectable)
    {
        if (selectable is TowerSelectable towerSelectable)
        {
            if (towerSelectable.TryGetComponent(out Tower tower))
                ShowUI(tower);
        }
        else
        {
            HideUI();
        }
    }

    private void OnTowerDeselected(IGameSelectable selectable)
    {
        if (selectable is TowerSelectable)
            HideUI();
    }

    private void ShowUI(Tower tower)
    {
        if (currentTower != null)
            currentTower.OnUpgraded -= OnTowerUpgraded;

        currentTower = tower;

        if (currentTower != null)
            currentTower.OnUpgraded += OnTowerUpgraded;

        if (uiContainer != null)
        {
            uiContainer.SetActive(true);
            AnimateScaleIn();
        }

        UpdateUpgradeButton();
    }

    private void HideUI()
    {
        if (currentTower != null)
            currentTower.OnUpgraded -= OnTowerUpgraded;

        fillMotionHandle.TryCancel();
        currentTower = null;

        if (uiContainer != null)
            AnimateScaleOut();
    }

    private void OnUpgradeButtonClicked()
    {
        if (currentTower != null && currentTower.TryUpgrade())
            UpdateUpgradeButton();
    }

    private void OnTowerUpgraded(Tower tower)
    {
        if (currentTower == tower)
        {
            UpdateUpgradeButton();
            HideUI();
        }
    }

    private void OnCurrencyUpdated(int newCurrency)
    {
        if (currentTower != null)
            UpdateUpgradeButton();
    }

    private void UpdateUpgradeButton()
    {
        if (upgradeButton == null || upgradeCostText == null || currentTower == null)
            return;

        bool canUpgrade = currentTower.CanUpgrade();

        if (canUpgrade)
        {
            int cost = currentTower.GetUpgradeCost();
            bool canAfford = EconomyManager.Instance.CanAfford(cost);

            upgradeCostText.SetText($"{cost}");
            upgradeCostText.color = canAfford ? canAffordTextColor : cannotAffordTextColor;

            if (fillImage != null)
            {
                int currentCurrency = EconomyManager.Instance.GetCurrencyAmount();
                float targetFillAmount = Mathf.Clamp01((float)currentCurrency / cost);

                fillMotionHandle.TryCancel();
                fillMotionHandle = LMotion
                    .Create(fillImage.fillAmount, targetFillAmount, fillDuration)
                    .WithEase(fillEase)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .BindToFillAmount(fillImage)
                    .AddTo(gameObject);
            }
        }
        else
        {
            upgradeCostText.SetText("MAX");
            upgradeCostText.color = cannotAffordTextColor;

            if (fillImage != null)
            {
                fillMotionHandle.TryCancel();
                fillMotionHandle = LMotion
                    .Create(fillImage.fillAmount, 0f, fillDuration)
                    .WithEase(fillEase)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .BindToFillAmount(fillImage)
                    .AddTo(gameObject);
            }
        }
    }

    private void AnimateScaleIn()
    {
        if (uiContainer == null || flexalonObject == null)
            return;

        scaleMotionHandle.TryCancel();
        fadeMotionHandle.TryCancel();
        if (scaleOutCoroutine != null)
        {
            StopCoroutine(scaleOutCoroutine);
            scaleOutCoroutine = null;
        }

        Vector3 initialScale = new(0.05f, 0.2f, 0.3f);
        flexalonObject.Scale = initialScale;
        canvasGroup.alpha = 0f;

        scaleMotionHandle = LMotion
            .Create(flexalonObject.Scale, originalScale, scaleDuration)
            .WithEase(scaleEase)
            .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
            .BindToFlexalonScale(flexalonObject)
            .AddTo(gameObject);

        if (canvasGroup != null)
        {
            fadeMotionHandle = LMotion
                .Create(0f, 1f, fadeDuration)
                .WithEase(fadeEase)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .BindToAlpha(canvasGroup)
                .AddTo(gameObject);
        }
    }

    private void AnimateScaleOut()
    {
        if (uiContainer == null || flexalonObject == null)
            return;

        scaleMotionHandle.TryCancel();
        fadeMotionHandle.TryCancel();

        scaleMotionHandle = LMotion
            .Create(flexalonObject.Scale, Vector3.zero, scaleDuration)
            .WithEase(scaleEase)
            .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
            .BindToFlexalonScale(flexalonObject)
            .AddTo(gameObject);

        if (canvasGroup != null)
        {
            float currentAlpha = canvasGroup.alpha;
            fadeMotionHandle = LMotion
                .Create(currentAlpha, 0f, fadeDuration)
                .WithEase(fadeEase)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .BindToAlpha(canvasGroup)
                .AddTo(gameObject);
        }

        // Start coroutine to deactivate after animation completes
        if (scaleOutCoroutine != null)
            StopCoroutine(scaleOutCoroutine);
        scaleOutCoroutine = StartCoroutine(DeactivateAfterScaleOut());
    }

    private IEnumerator DeactivateAfterScaleOut()
    {
        float maxDuration = Mathf.Max(scaleDuration, fadeDuration);
        yield return new WaitForSecondsRealtime(maxDuration);
        if (uiContainer != null)
            uiContainer.SetActive(false);
        scaleOutCoroutine = null;
    }

    private void OnDestroy()
    {
        SelectionSystem.OnSelected -= OnTowerSelected;
        SelectionSystem.OnDeselected -= OnTowerDeselected;
        EconomyManager.OnCurrencyUpdated -= OnCurrencyUpdated;

        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);

        scaleMotionHandle.TryCancel();
        fadeMotionHandle.TryCancel();
        if (scaleOutCoroutine != null)
            StopCoroutine(scaleOutCoroutine);
    }
}
