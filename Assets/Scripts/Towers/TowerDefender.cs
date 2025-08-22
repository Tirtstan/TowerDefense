using UnityEngine;

public class TowerDefender : MonoBehaviour, ITowerAttack
{
    [Header("Components")]
    [SerializeField]
    private TowerSO towerSO;
    private Collider rangeCollider;

    private void Awake()
    {
        rangeCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other) { }

    public void Attack()
    {
        Debug.Log($"Attacking with {towerSO.Name} for {towerSO.Damage} damage.");
    }
}
