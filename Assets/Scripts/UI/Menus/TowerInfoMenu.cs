using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TowerInfoMenu : Singleton<TowerInfoMenu>
{
    [Header("Components")]
    [Header("Images")]
    [SerializeField]
    private Image towerThumbnail;

    [SerializeField]
    private Image healthFillImage;

    [Header("Text")]
    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private TextMeshProUGUI descriptionText;

    [SerializeField]
    private TextMeshProUGUI healthText;

    [SerializeField]
    private TextMeshProUGUI levelText;

    [SerializeField]
    private TextMeshProUGUI statsText;

    [Header("Buttons")]
    [SerializeField]
    private Button closeButton;
    private Tower currentTower;

    private void OnEnable()
    {
        Tower.OnHealthChanged += OnTowerHealthChanged;
    }

    private void OnTowerHealthChanged(Tower tower)
    {
        if (tower == currentTower)
            UpdateHealthDisplay(currentTower.GetCurrentHealth(), currentTower.GetTowerSO().Stats.Health);
    }

    public void ShowMenu(Tower tower)
    {
        gameObject.SetActive(true);
        UpdateDisplay(tower);
    }

    private void UpdateDisplay(Tower tower)
    {
        if (tower == null)
            return;

        currentTower = tower;
        TowerStats stats = tower.GetTowerSO().Stats;

        nameText.SetText(tower.GetTowerSO().Name);
        descriptionText.SetText(tower.GetTowerSO().Description);
        levelText.SetText($"Lvl. {1}"); // TODO: Implement level system

        string statsString =
            $"Cost: {stats.Cost}\n"
            + $"Damage: {stats.Damage}\n"
            + $"Range: {stats.Range}\n"
            + $"Attack Interval: {stats.AttackInterval}s";

        statsText.SetText(statsString);

        towerThumbnail.sprite = tower.GetTowerSO().Sprite;
        UpdateHealthDisplay(tower.GetCurrentHealth(), stats.Health);
    }

    private void UpdateHealthDisplay(float currentHealth, float maxHealth)
    {
        healthText.SetText($"{currentHealth} / {maxHealth}");
        healthFillImage.fillAmount = currentHealth / maxHealth;
    }

    public void HideMenu() => gameObject.SetActive(false);

    private void OnDisable()
    {
        Tower.OnHealthChanged -= OnTowerHealthChanged;
    }
}
