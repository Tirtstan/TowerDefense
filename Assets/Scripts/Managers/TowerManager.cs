using System.Collections.Generic;
using System.Linq;
using QFSW.QC;
using UnityEngine;

[CommandPrefix("tower.")]
public class TowerManager : Singleton<TowerManager>
{
    private readonly Dictionary<TowerSO, int> towersPlaced = new();
    private readonly List<Tower> allTowers = new();

    protected override void Awake()
    {
        base.Awake();
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState state)
    {
        if (state == GameState.MainMenu)
            DestroyAllTowers();
    }

    public void RegisterTower(Tower tower)
    {
        if (!allTowers.Contains(tower))
            allTowers.Add(tower);

        TowerSO towerSO = tower.GetTowerSO();
        if (towersPlaced.ContainsKey(towerSO))
            towersPlaced[towerSO]++;
        else
            towersPlaced[towerSO] = 1;
    }

    public void UnregisterTower(Tower tower)
    {
        allTowers.Remove(tower);

        TowerSO towerSO = tower.GetTowerSO();
        if (towersPlaced.ContainsKey(towerSO))
        {
            towersPlaced[towerSO]--;
            if (towersPlaced[towerSO] <= 0)
                towersPlaced.Remove(towerSO);
        }
    }

    [Command("destroy_all_towers", "Destroys all towers in the game.")]
    public void DestroyAllTowers()
    {
        var towersToDestroy = allTowers.ToList();
        foreach (var tower in towersToDestroy)
        {
            if (tower != null)
                Destroy(tower.gameObject);
        }

        allTowers.Clear();
        towersPlaced.Clear();
    }

    public Dictionary<TowerSO, int> GetTowersPlaced() => towersPlaced;

    [Command("try_upgrade_tower", "Attempts to upgrade the specified tower.")]
    public bool TryUpgradeTower(Tower tower) => tower.TryUpgrade();

    [Command("get_tower_stats", "Gets the effective stats of the specified tower.")]
    public string GetTowerStatsString(Tower tower)
    {
        TowerStats stats = tower.GetEffectiveStats();
        return stats.ToString();
    }
}
