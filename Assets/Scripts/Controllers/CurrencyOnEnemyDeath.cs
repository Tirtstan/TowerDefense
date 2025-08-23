using UnityEngine;

public class CurrencyOnEnemyDeath : MonoBehaviour
{
    private void Awake()
    {
        Enemy.OnDeath += OnEnemyDeath;
    }

    private void OnEnemyDeath(EnemySO enemy)
    {
        EconomyManager.Instance.AddCurrency(enemy.SoulAmount);
    }

    private void OnDestroy()
    {
        Enemy.OnDeath -= OnEnemyDeath;
    }
}
