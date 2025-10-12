using UnityEngine;

public class BallistaAimer : TowerAimer
{
    [Header("Visuals")]
    [SerializeField]
    private Transform yAxisBody;

    [SerializeField]
    private Transform xAxisLauncher;

    [Header("Configs")]
    [SerializeField]
    [Range(1f, 50f)]
    private float turnRate = 10f;

    public override void AimAt(Transform target)
    {
        if (target == null)
            return;

        if (yAxisBody != null)
        {
            Vector3 bodyDirection = target.position - yAxisBody.position;
            bodyDirection.y = 0;
            Quaternion yRotation = Quaternion.LookRotation(bodyDirection);
            yAxisBody.rotation = Quaternion.Slerp(yAxisBody.rotation, yRotation, Time.deltaTime * turnRate);
        }

        if (xAxisLauncher != null)
        {
            Vector3 launcherDirection = target.position - xAxisLauncher.position;
            Quaternion targetRotation = Quaternion.LookRotation(launcherDirection);

            float targetXAngle = targetRotation.eulerAngles.x;
            Quaternion xRotation = Quaternion.Euler(targetXAngle, 0, 0);

            xAxisLauncher.localRotation = Quaternion.Slerp(
                xAxisLauncher.localRotation,
                xRotation,
                Time.deltaTime * turnRate
            );
        }
    }
}
