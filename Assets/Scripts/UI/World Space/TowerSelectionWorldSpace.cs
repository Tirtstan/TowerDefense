using LitMotion;
using LitMotion.Extensions;
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
    private Tower currentTower;
    private MotionHandle fillMotionHandle;

    protected override void Awake()
    {
        base.Awake();

        if (uiContainer != null)
            uiContainer.SetActive(false);

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
            uiContainer.SetActive(true);

        UpdateUpgradeButton();
    }

    private void HideUI()
    {
        if (currentTower != null)
            currentTower.OnUpgraded -= OnTowerUpgraded;

        fillMotionHandle.TryCancel();
        currentTower = null;

        if (uiContainer != null)
            uiContainer.SetActive(false);
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

    private void OnDestroy()
    {
        SelectionSystem.OnSelected -= OnTowerSelected;
        SelectionSystem.OnDeselected -= OnTowerDeselected;
        EconomyManager.OnCurrencyUpdated -= OnCurrencyUpdated;

        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);
    }
}
