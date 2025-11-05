using Flexalon;
using LitMotion;
using LitMotion.Extensions.Flexalon;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(FlexalonObject))]
public class ScaleOnHover
    : MonoBehaviour,
        ISelectHandler,
        IDeselectHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler
{
    [Header("Animation")]
    [SerializeField]
    [Range(1.0f, 2.0f)]
    private float scaleFactor = 1.1f;

    [SerializeField]
    [Range(0.05f, 1f)]
    private float duration = 0.2f;

    [SerializeField]
    private Ease ease = Ease.OutExpo;
    private FlexalonObject flexObj;
    private Vector3 originalScale;
    private MotionHandle currentMotionHandle;

    private void Awake()
    {
        flexObj = GetComponent<FlexalonObject>();
        originalScale = flexObj.Scale;
    }

    public void OnSelect(BaseEventData eventData) => ScaleUpTween();

    public void OnDeselect(BaseEventData eventData) => ResetToOriginalTween();

    public void OnPointerEnter(PointerEventData eventData) => ScaleUpTween();

    public void OnPointerExit(PointerEventData eventData) => ResetToOriginalTween();

    public void OnPointerClick(PointerEventData eventData) => ResetToOriginalTween();

    private void ScaleUpTween()
    {
        currentMotionHandle.TryCancel();
        currentMotionHandle = LMotion
            .Create(flexObj.Scale, originalScale * scaleFactor, duration)
            .WithEase(ease)
            .BindToFlexalonScale(flexObj)
            .AddTo(gameObject);
    }

    private void ResetToOriginalTween()
    {
        currentMotionHandle.TryCancel();
        currentMotionHandle = LMotion
            .Create(flexObj.Scale, originalScale, duration)
            .WithEase(ease)
            .BindToFlexalonScale(flexObj)
            .AddTo(gameObject);
    }
}
