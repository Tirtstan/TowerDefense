using LitMotion;
using UnityEngine;

public class CurrencyShaderController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    [Tooltip("The GenerateCurrency component to track")]
    private GenerateCurrency generateCurrency;

    [SerializeField]
    [Tooltip(
        "The renderer that contains the material with the shader property. Children renderers will also be updated."
    )]
    private Renderer targetRenderer;

    [Header("Shader Settings")]
    [SerializeField]
    [Tooltip("Name of the shader property to update (e.g., _Progress, _FillAmount)")]
    private string shaderPropertyName = "_Progress";

    [SerializeField]
    [Tooltip("Duration of interpolation between shader values (lower = faster)")]
    [Range(0.01f, 2f)]
    private float duration = 0.2f;

    [SerializeField]
    private Ease ease = Ease.OutSine;

    private int shaderPropertyID;
    private Renderer[] allRenderers;
    private float currentShaderValue;
    private MotionHandle motionHandle;

    private void Awake()
    {
        if (generateCurrency == null)
            generateCurrency = GetComponent<GenerateCurrency>();

        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        shaderPropertyID = Shader.PropertyToID(shaderPropertyName);

        CacheRenderers();
        currentShaderValue = 0f;
    }

    private void CacheRenderers()
    {
        if (targetRenderer != null)
        {
            allRenderers = targetRenderer.GetComponentsInChildren<Renderer>(true);
        }
        else
        {
            allRenderers = new Renderer[0];
        }
    }

    private void OnEnable()
    {
        if (generateCurrency != null)
            generateCurrency.OnCurrencyAmountChanged += UpdateShaderProperty;
    }

    private void OnDisable()
    {
        if (generateCurrency != null)
            generateCurrency.OnCurrencyAmountChanged -= UpdateShaderProperty;

        motionHandle.TryCancel();
    }

    private void Start()
    {
        UpdateShaderProperty(0, 0f);
    }

    private void UpdateShaderProperty(int amount, float percentage)
    {
        if (allRenderers == null || allRenderers.Length == 0)
            return;

        // Cancel any existing motion
        motionHandle.TryCancel();

        // Create new motion from current value to target value
        motionHandle = LMotion
            .Create(currentShaderValue, percentage, duration)
            .WithEase(ease)
            .Bind(value =>
            {
                currentShaderValue = value;
                ApplyShaderValue(value);
            })
            .AddTo(gameObject);
    }

    private void ApplyShaderValue(float value)
    {
        if (allRenderers == null || allRenderers.Length == 0)
            return;

        foreach (Renderer renderer in allRenderers)
        {
            if (renderer == null)
                continue;

            renderer.material.SetFloat(shaderPropertyID, value);
        }
    }

    private void Reset()
    {
        if (generateCurrency == null)
            generateCurrency = GetComponent<GenerateCurrency>();

        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();
    }
}
