using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SpinOnClick : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private RectTransform target;

    [Header("Animation")]
    [SerializeField]
    private float duration = 0.5f;

    [SerializeField]
    private Ease ease = Ease.OutCubic;
    private Button button;
    private float originalRotZ;
    private MotionHandle motionHandle;

    private void Awake()
    {
        button = GetComponent<Button>();
        originalRotZ = target.rotation.z;
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        motionHandle.TryCancel();
        motionHandle = LMotion
            .Create(target.eulerAngles.z, originalRotZ + 359f, duration)
            .WithEase(ease)
            .WithOnComplete(
                () => target.eulerAngles = new Vector3(target.eulerAngles.x, target.eulerAngles.y, originalRotZ)
            )
            .BindToEulerAnglesZ(target)
            .AddTo(gameObject);
    }
}
