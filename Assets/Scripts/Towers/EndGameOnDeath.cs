using UnityEngine;

[RequireComponent(typeof(TowerHealth))]
public class EndGameOnDeath : MonoBehaviour
{
    private TowerHealth health;

    private void Awake()
    {
        health = GetComponent<TowerHealth>();
        health.OnDeath += HandleTowerDeath;
    }

    private void HandleTowerDeath()
    {
        GameManager.Instance.EndGame();
    }

    private void OnDestroy()
    {
        health.OnDeath -= HandleTowerDeath;
    }
}
