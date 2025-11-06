using UnityEngine;

public class ParticleOnRangeAttack : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Tower tower;

    [SerializeField]
    private TowerAttack towerAttack;

    [SerializeField]
    private ParticleSystem placeParticleSystem;

    private void Awake()
    {
        towerAttack.OnAttack += HandleOnAttack;
    }

    private void HandleOnAttack()
    {
        // var shape = placeParticleSystem.shape;
        // shape.radius = tower.GetEffectiveStats().Range;

        placeParticleSystem.Play();
    }

    private void OnDestroy()
    {
        towerAttack.OnAttack -= HandleOnAttack;
    }
}
