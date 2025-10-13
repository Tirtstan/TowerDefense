using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Scriptable Objects/Enemy")]
public class EnemySO : ScriptableObject
{
    [Header("Enemy Stats")]
    public string Name = "Enemy";
    public float Health = 100f;

    [Space]
    public float Damage = 5f;
    public float Speed = 3.5f;

    [Space]
    public float VisionRange = 3.5f;
    public float AttackRange = 1.5f;

    [Tooltip("The interval between each attack in seconds.")]
    public float AttackInterval = 1f;

    [Header("Spawning")]
    [Tooltip("The amount of currency provided by this enemy upon death.")]
    public int CurrencyDropAmount = 1;

    [Tooltip("Rating of how difficult this enemy is. Used for balancing waves.")]
    public float DifficultyRating = 1f;
}
