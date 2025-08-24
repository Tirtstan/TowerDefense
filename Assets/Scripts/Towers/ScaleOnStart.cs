using DG.Tweening;
using UnityEngine;

public class ScaleOnStart : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField]
    [Range(0.01f, 3f)]
    private float duration = 0.1f;

    [SerializeField]
    [Range(0.01f, 1f)]
    private float strength = 0.2f;

    [SerializeField]
    private Ease ease = Ease.OutBack;

    private void Start()
    {
        transform.DOPunchScale(Vector3.one * strength, duration).SetEase(ease);
    }
}
