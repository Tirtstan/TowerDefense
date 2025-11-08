using UnityEngine;

public class CurrencyOnDeath : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private EnemyHealth enemyHealth;

    private void Awake()
    {
        enemyHealth.OnDeath += HandleDeath;
    }

    private void HandleDeath()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.Deposit(enemyHealth.GetEnemySO().CurrencyDropAmount);
    }

    private void OnDestroy()
    {
        enemyHealth.OnDeath -= HandleDeath;
    }

    private void Reset()
    {
        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();
    }
}
