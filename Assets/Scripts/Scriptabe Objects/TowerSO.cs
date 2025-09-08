using UnityEngine;

[CreateAssetMenu(fileName = "Tower", menuName = "Scriptable Objects/Tower")]
public class TowerSO : ScriptableObject
{
    [Header("Properties")]
    public int Id;
    public string Name = "Tower";

    [TextArea]
    public string Description;
    public LayerMask PlaceableLayer;
    public LayerMask EnemyLayer;

    [Space]
    public Sprite Sprite;
    public Tower Prefab;
    public Tower PrefabPreview;
    public GameObject OriginalMesh;

    [Header("Stats")]
    public TowerStats Stats;
}
