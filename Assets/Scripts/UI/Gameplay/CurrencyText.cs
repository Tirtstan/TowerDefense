using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CurrencyText : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField]
    [Range(0.1f, 2f)]
    private float duration = 0.35f;

    [SerializeField]
    private Ease ease = Ease.OutSine;
    private TextMeshProUGUI text;
    private MotionHandle motionHandle;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        EconomyManager.OnCurrencyUpdated += UpdateCurrencyText;
    }

    public void UpdateCurrencyText(int amount)
    {
        int start = int.Parse(text.text);

        motionHandle.TryCancel();
        motionHandle = LMotion.Create(start, amount, duration).WithEase(ease).BindToText(text).AddTo(gameObject);
    }

    private void OnDisable()
    {
        EconomyManager.OnCurrencyUpdated -= UpdateCurrencyText;
    }
}
