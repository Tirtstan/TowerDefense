using UnityEngine;

[CreateAssetMenu(fileName = "Tower", menuName = "Scriptable Objects/Tower")]
public class TowerSO : ScriptableObject
{
    [Header("Properties")]
    public string Name = "Tower";

    [TextArea]
    public string Description;

    [Space]
    public Sprite Sprite;
    public Tower Prefab;
    public Tower PrefabPreview;

    [Header("Stats")]
    public TowerStats Stats;
}
