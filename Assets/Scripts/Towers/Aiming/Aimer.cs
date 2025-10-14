using UnityEngine;

public abstract class Aimer : MonoBehaviour, ITowerAimer
{
    public abstract void AimAt(Transform target);
}
