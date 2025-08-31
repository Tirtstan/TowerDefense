using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(IAttack))]
public class AttackShake : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private CinemachineImpulseSource cinemachineImpulseSource;
    private IAttack towerAttack;

    private void Awake()
    {
        towerAttack = GetComponent<IAttack>();
    }

    private void OnEnable()
    {
        towerAttack.OnAttack += HandleAttack;
    }

    private void HandleAttack() => cinemachineImpulseSource.GenerateImpulse();

    private void OnDisable()
    {
        towerAttack.OnAttack -= HandleAttack;
    }
}
