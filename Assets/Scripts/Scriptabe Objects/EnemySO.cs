using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Scriptable Objects/Enemy")]
public class EnemySO : ScriptableObject
{
    [Header("Enemy Stats")]
    public string Name = "Enemy";
    public float Health = 100f;
    public float Damage = 5f;
    public float Speed = 3.5f;
    public float VisionRange = 3.5f;

    [Tooltip("The interval between each attack in seconds.")]
    public float AttackInterval = 1f;

    [Tooltip("The amount of souls provided by this enemy upon death.")]
    public int SoulAmount = 1;
}
