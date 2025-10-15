using UnityEngine;

public class ArcAimer : Aimer
{
    [Header("Visuals")]
    [SerializeField]
    private Transform yAxisBody;

    [SerializeField]
    private Transform xAxisLauncher;

    [Header("Configs")]
    [SerializeField]
    private Vector2 xAngleRange = new(0f, 45f);

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
            if (bodyDirection != Vector3.zero)
            {
                Quaternion yRotation = Quaternion.LookRotation(bodyDirection);
                yAxisBody.rotation = Quaternion.Slerp(yAxisBody.rotation, yRotation, Time.deltaTime * turnRate);
            }
        }

        if (xAxisLauncher != null)
        {
            Vector3 launcherDirection = target.position - xAxisLauncher.position;
            Quaternion targetRotation = Quaternion.LookRotation(launcherDirection);

            float targetXAngle = targetRotation.eulerAngles.x;
            if (targetXAngle > 180)
                targetXAngle -= 360;

            float clampedXAngle = Mathf.Clamp(targetXAngle, -xAngleRange.y, -xAngleRange.x);
            Quaternion xRotation = Quaternion.Euler(clampedXAngle, 0, 0);
            xAxisLauncher.localRotation = Quaternion.Slerp(
                xAxisLauncher.localRotation,
                xRotation,
                Time.deltaTime * turnRate
            );
        }
    }
}
