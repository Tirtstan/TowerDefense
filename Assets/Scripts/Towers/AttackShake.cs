using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(ITowerAttack))]
public class AttackShake : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private CinemachineImpulseSource cinemachineImpulseSource;
    private ITowerAttack towerAttack;

    private void Awake()
    {
        towerAttack = GetComponent<ITowerAttack>();
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
