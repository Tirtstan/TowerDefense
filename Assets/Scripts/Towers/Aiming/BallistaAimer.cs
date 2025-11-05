using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

public class BallistaAimer : Aimer
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
    private bool canAim;

    private void OnEnable()
    {
        if (yAxisBody != null)
        {
            LMotion
                .Create(Vector3.zero, Vector3.up * 360f, 0.9f)
                .WithEase(Ease.OutCubic)
                .WithOnComplete(() => canAim = true)
                .BindToEulerAngles(yAxisBody)
                .AddTo(gameObject);
        }
        else
        {
            canAim = true;
        }
    }

    public override void AimAt(Transform target)
    {
        if (target == null || !canAim)
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
