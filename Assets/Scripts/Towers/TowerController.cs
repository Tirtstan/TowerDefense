using UnityEngine;

[RequireComponent(typeof(ITowerAttack))]
public class TowerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private TowerSO towerSO;

    [SerializeField]
    private LayerMask enemyLayerMask = -1;

    private const int MaxHitColliders = 50;
    private ITowerAttack towerAttack;
    private float currentTime;

    private void Awake()
    {
        towerAttack = GetComponent<ITowerAttack>();
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= towerSO.AttackInterval)
        {
            currentTime = 0;
            AttackAllTargetsInRange();
        }
    }

    private void AttackAllTargetsInRange()
    {
        Collider[] hitColliders = new Collider[MaxHitColliders];
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, towerSO.Range, hitColliders, enemyLayerMask);

        for (int i = 0; i < hitCount; i++)
        {
            if (hitColliders[i].TryGetComponent(out IDamagable damagable))
                towerAttack.Attack(damagable);
        }
    }
}
