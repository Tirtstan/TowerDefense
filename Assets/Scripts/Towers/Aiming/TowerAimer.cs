using UnityEngine;

public abstract class TowerAimer : MonoBehaviour, ITowerAimer
{
    public abstract void AimAt(Transform target);
}
