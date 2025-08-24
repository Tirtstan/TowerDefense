using UnityEngine;

[CreateAssetMenu(fileName = "Tower", menuName = "Scriptable Objects/Tower")]
public class TowerSO : ScriptableObject
{
    [Header("Properties")]
    public string Name = "Tower";
    public Sprite Sprite;
    public Tower Prefab;

    [Header("Stats")]
    public float Health = 100;
    public int Cost = 100;
    public float Damage = 10;
    public float Range = 5;

    [Tooltip("The interval between each attack in seconds.")]
    public float AttackInterval = 1;
}
