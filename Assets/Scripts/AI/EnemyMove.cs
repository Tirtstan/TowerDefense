using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMove : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private EnemySO enemySO;
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = enemySO.Speed;
    }

    private void Start()
    {
        Vector3 targetPosition = CenterTower.Instance.GetPosition();
        agent.SetDestination(targetPosition);
    }
}
