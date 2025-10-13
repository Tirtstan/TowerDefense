using UnityEngine;

[CreateAssetMenu(fileName = "Splash Projectile", menuName = "Scriptable Objects/Splash Projectile")]
public class SplashProjectileSO : ProjectileSO
{
    [Header("Splash Projectile Properties")]
    public float SplashRadius = 2f;

    [Range(0f, 1f)]
    public float SplashDamageMultiplier = 0.3f;
    public float SplashForce = 5f;
}
