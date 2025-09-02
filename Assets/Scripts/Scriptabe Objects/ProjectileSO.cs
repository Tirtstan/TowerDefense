using UnityEngine;

[CreateAssetMenu(fileName = "Projectile", menuName = "Scriptable Objects/Projectile")]
public class ProjectileSO : ScriptableObject
{
    [Header("Properties")]
    [Range(0f, 10f)]
    public float Lifetime = 5f;
    public float Speed = 10f;
    public LayerMask HitLayers;

    [Range(0f, 10f)]
    public float MinYPosition = 0.25f;
}
