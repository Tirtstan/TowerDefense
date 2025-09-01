using DG.Tweening;
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

    private void OnTowerHealthChanged(IDamagable damagable)
    {
        if (currentHealth.GetTowerSO() == currentTower.GetTowerSO())
            UpdateHealthDisplay(currentHealth.CurrentHealth, currentHealth.MaxHealth);
    }

    public void ShowMenu(Tower tower)
    {
        currentTower = tower;
        if (currentTower.TryGetComponent(out currentHealth))
            currentHealth.OnHealthChanged += OnTowerHealthChanged;

        menu.SetActive(true);
        UpdateDisplay(tower);
    }

    private void UpdateDisplay(Tower tower)
    {
        if (tower == null)
            return;

        TowerStats stats = tower.GetTowerSO().Stats;

        nameText.SetText(tower.GetTowerSO().Name);
        levelText.SetText($"Lvl. {1}"); // TODO: Implement level system

        damageText.SetText($"{stats.Damage} damage");
        rangeText.SetText($"{stats.Range} metre(s)");
        attackIntervalText.SetText($"{stats.AttackInterval:0.0} sec(s)");

        UpdateHealthDisplay(currentHealth.CurrentHealth, stats.Health);
    }

    private void UpdateHealthDisplay(float currentHealth, float maxHealth)
    {
        healthText.SetText($"{currentHealth} / {maxHealth}");

        healthFillImage.DOKill();
        healthFillImage.DOFillAmount(currentHealth / maxHealth, healthFillDuration).SetEase(healthEase);
    }

    public void HideMenu()
    {
        if (currentHealth != null)
            currentHealth.OnHealthChanged -= OnTowerHealthChanged;

        currentTower = null;
        currentHealth = null;
        menu.SetActive(false);
    }
}
