using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(IAttack))]
public class AttackShake : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Transform visual;

    [SerializeField]
    private CinemachineImpulseSource cinemachineImpulseSource;

    [Header("Configs")]
    [SerializeField]
    [Range(0.1f, 1f)]
    private float shakeForce = 0.1f;

    [SerializeField]
    [Range(0.1f, 1f)]
    private float punchForce = 0.2f;

    [SerializeField]
    [Range(0.1f, 1f)]
    private float shakeDuration = 0.2f;
    private IAttack towerAttack;

    private void Awake()
    {
        towerAttack = GetComponent<IAttack>();
    }

    private void OnEnable()
    {
        towerAttack.OnAttack += HandleAttack;
    }

    private void HandleAttack()
    {
        visual.DOPunchScale(new Vector3(0.25f, -0.75f, 0.25f) * punchForce, shakeDuration, 1, 0.5f);
        cinemachineImpulseSource.GenerateImpulse(GetRandomVelocity() * shakeForce);
    }

    private Vector3 GetRandomVelocity() =>
        new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

    private void OnDisable()
    {
        visual.DOKill();
        towerAttack.OnAttack -= HandleAttack;
    }
}
