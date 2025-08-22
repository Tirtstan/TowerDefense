using UnityEngine;

[CreateAssetMenu(fileName = "Tower", menuName = "Scriptable Objects/Tower")]
public class TowerSO : ScriptableObject
{
    [Header("Properties")]
    public string Name = "Tower";
    public Sprite Sprite;
    public GameObject Prefab;

    [Header("Stats")]
    public float Health = 100;
    public float Cost = 100;
    public float Damage = 10;
}
