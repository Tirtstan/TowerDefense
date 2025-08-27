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
    private Image towerThumbnail;

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

    protected override void Awake()
    {
        base.Awake();
        HideMenu();
    }

    private void OnTowerHealthChanged(Tower tower)
    {
        if (tower == currentTower)
            UpdateHealthDisplay(currentTower.GetCurrentHealth(), currentTower.GetTowerSO().Stats.Health);
    }

    public void ShowMenu(Tower tower)
    {
        Tower.OnHealthChanged += OnTowerHealthChanged;
        menu.SetActive(true);
        UpdateDisplay(tower);
    }

    private void UpdateDisplay(Tower tower)
    {
        if (tower == null)
            return;

        currentTower = tower;
        TowerStats stats = tower.GetTowerSO().Stats;

        nameText.SetText(tower.GetTowerSO().Name);
        levelText.SetText($"Lvl. {1}"); // TODO: Implement level system

        damageText.SetText($"{stats.Damage} damage");
        rangeText.SetText($"{stats.Range} metres");
        attackIntervalText.SetText($"{stats.AttackInterval:0.0} sec(s)");

        towerThumbnail.sprite = tower.GetTowerSO().Sprite;
        UpdateHealthDisplay(tower.GetCurrentHealth(), stats.Health);
    }

    private void UpdateHealthDisplay(float currentHealth, float maxHealth)
    {
        healthText.SetText($"{currentHealth} / {maxHealth}");
        healthFillImage.fillAmount = currentHealth / maxHealth;
    }

    public void HideMenu()
    {
        Tower.OnHealthChanged -= OnTowerHealthChanged;
        menu.SetActive(false);
    }
}
