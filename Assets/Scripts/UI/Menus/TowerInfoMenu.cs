using System.Collections.Generic;
using DG.Tweening;
using EasyTextEffects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TowerInfoMenu : Singleton<TowerInfoMenu>
{
    [Header("Components")]
    [SerializeField]
    private GameObject menu;

    [Header("Images")]
    [SerializeField]
    private Image healthFillImage;

    [Header("Text")]
    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private TextMeshProUGUI healthText;

    [SerializeField]
    private TextMeshProUGUI levelText;

    [SerializeField]
    private TextMeshProUGUI damageText;

    [SerializeField]
    private TextMeshProUGUI rangeText;

    [SerializeField]
    private TextMeshProUGUI attackIntervalText;

    [SerializeField]
    private TextMeshProUGUI layerPlacementText;

    [Header("Text Effects")]
    [SerializeField]
    private TextEffect nameTextEffect;

    [SerializeField]
    private TextEffect healthTextEffect;

    [Header("Animation")]
    [SerializeField]
    [Range(0, 1)]
    private float healthFillDuration = 0.25f;

    [SerializeField]
    private Ease healthEase = Ease.OutCubic;
    private Tower currentTower;
    private TowerHealth currentHealth;

    protected override void Awake()
    {
        base.Awake();
        HideMenu();
    }

    private void OnTowerUpgraded(Tower tower)
    {
        if (currentTower == tower)
            UpdateDisplay(tower);
    }

    private void OnTowerHealthChanged(IDamagable damagable)
    {
        if (currentHealth.GetTowerSO() == currentTower.GetTowerSO())
            UpdateHealthDisplay(currentHealth.CurrentHealth, currentHealth.MaxHealth);
    }

    public void ShowMenu(Tower tower)
    {
        currentTower = tower;
        if (currentTower.TryGetComponent(out currentHealth))
        {
            currentHealth.OnHealthChanged += OnTowerHealthChanged;
            currentHealth.OnDeath += HideMenu;
        }

        currentTower.OnUpgraded += OnTowerUpgraded;

        menu.SetActive(true);
        UpdateDisplay(tower);
    }

    private void UpdateDisplay(Tower tower)
    {
        if (tower == null)
            return;

        TowerStats stats = tower.GetEffectiveStats();

        nameText.SetText(tower.GetTowerSO().Name);
        nameTextEffect.Refresh();

        string levelString = "+";
        for (int i = 0; i < tower.CurrentLevel; i++)
            levelString += "+";

        levelText.SetText(levelString);

        damageText.SetText($"{stats.Damage:0.##} damage");
        rangeText.SetText($"{stats.Range:0.0} metre(s)");
        attackIntervalText.SetText($"{stats.AttackInterval:0.0} sec(s)");

        string layers = GetLayerNames(tower.GetTowerSO().PlaceableLayer);
        layerPlacementText.SetText(layers);

        UpdateHealthDisplay(currentHealth.CurrentHealth, stats.Health);
    }

    private void UpdateHealthDisplay(float currentHealth, float maxHealth)
    {
        healthText.SetText($"{currentHealth} / {maxHealth}");
        healthTextEffect.Refresh();

        healthFillImage.DOKill();
        healthFillImage.DOFillAmount(currentHealth / maxHealth, healthFillDuration).SetEase(healthEase);
    }

    public void HideMenu()
    {
        if (currentHealth != null)
        {
            currentHealth.OnHealthChanged -= OnTowerHealthChanged;
            currentHealth.OnDeath -= HideMenu;
        }

        if (currentTower != null)
        {
            currentTower.OnUpgraded -= OnTowerUpgraded;
        }

        currentTower = null;
        currentHealth = null;
        menu.SetActive(false);
    }

    public bool TryHideMenu(Tower tower)
    {
        if (currentTower == tower)
        {
            HideMenu();
            return true;
        }

        return false;
    }

    private string GetLayerNames(LayerMask layerMask)
    {
        List<string> layerNames = new();
        int maskValue = layerMask.value;

        for (int i = 0; i < 32; i++)
        {
            if ((maskValue & (1 << i)) != 0)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                    layerNames.Add(layerName);
            }
        }

        return layerNames.Count > 0 ? string.Join(", ", layerNames) : "None";
    }
}
