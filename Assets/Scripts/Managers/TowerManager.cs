using System.Collections.Generic;
using UnityEngine;

public class TowerManager : Singleton<TowerManager>
{
    private readonly Dictionary<TowerSO, int> towersPlaced = new();

    public void RegisterTower(Tower tower)
    {
        TowerSO towerSO = tower.GetTowerSO();
        if (towersPlaced.ContainsKey(towerSO))
            towersPlaced[towerSO]++;
        else
            towersPlaced[towerSO] = 1;
    }

    public void UnregisterTower(Tower tower)
    {
        TowerSO towerSO = tower.GetTowerSO();
        if (towersPlaced.ContainsKey(towerSO))
        {
            towersPlaced[towerSO]--;
            if (towersPlaced[towerSO] <= 0)
                towersPlaced.Remove(towerSO);
        }
    }

    public Dictionary<TowerSO, int> GetTowersPlaced() => towersPlaced;
}
