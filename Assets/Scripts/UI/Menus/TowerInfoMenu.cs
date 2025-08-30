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
    private Tower currentTower;
    private TowerHealth currentHealth;

    protected override void Awake()
    {
        base.Awake();
        HideMenu();
    }

    private void OnTowerHealthChanged(TowerHealth towerHealth)
    {
        if (currentHealth.GetTowerSO() == currentTower.GetTowerSO())
            UpdateHealthDisplay(currentHealth.GetCurrentHealth(), currentHealth.GetTowerSO().Stats.Health);
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
        rangeText.SetText($"{stats.Range} metres");
        attackIntervalText.SetText($"{stats.AttackInterval:0.0} sec(s)");

        UpdateHealthDisplay(currentHealth.GetCurrentHealth(), stats.Health);
    }

    private void UpdateHealthDisplay(float currentHealth, float maxHealth)
    {
        healthText.SetText($"{currentHealth} / {maxHealth}");
        healthFillImage.fillAmount = currentHealth / maxHealth;
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
