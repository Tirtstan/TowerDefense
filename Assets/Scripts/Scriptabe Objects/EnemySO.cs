using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Scriptable Objects/Enemy")]
public class EnemySO : ScriptableObject
{
    [Header("Enemy Stats")]
    public string Name = "Enemy";
    public float Health = 100f;
    public float Damage = 5f;
    public float AttackInterval = 1f;
}
