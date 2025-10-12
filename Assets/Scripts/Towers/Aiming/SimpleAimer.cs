using UnityEngine;

public class SimpleAimer : TowerAimer
{
    [Header("Configs")]
    [SerializeField]
    private Transform visualToRotate;

    [SerializeField]
    [Range(1f, 50f)]
    private float turnRate = 10f;

    public override void AimAt(Transform target)
    {
        if (visualToRotate == null || target == null)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(target.position - visualToRotate.position);
        visualToRotate.rotation = Quaternion.Slerp(visualToRotate.rotation, targetRotation, Time.deltaTime * turnRate);
    }
}
