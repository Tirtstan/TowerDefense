using UnityEngine;

[CreateAssetMenu(fileName = "Projectile", menuName = "Scriptable Objects/Projectile")]
public class ProjectileSO : ScriptableObject
{
    [Header("Visual Properties")]
    public float Lifetime = 5f;
    public float Speed = 10f;
    public float MinYPosition = 0.25f;

    [Header("Balancing")]
    public LayerMask HitLayers;
}
