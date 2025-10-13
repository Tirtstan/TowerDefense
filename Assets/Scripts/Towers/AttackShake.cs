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
        visual.DOPunchScale(Vector3.one * 0.1f, 0.1f, 1, 0);
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
