using LitMotion;
using UnityEngine;

public class TowerUpgradeShaderController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    [Tooltip("The Tower component to track for upgrades")]
    private Tower tower;

    [SerializeField]
    [Tooltip(
        "The parent transform to search for renderers. All renderers on this transform and its children will be updated."
    )]
    private Transform renderers;

    [Header("Shader Settings")]
    [SerializeField]
    [Tooltip("Name of the shader property for upgrade intensity (should be _UpgradeIntensity)")]
    private string intensityPropertyName = "_UpgradeIntensity";

    [SerializeField]
    [Tooltip("Duration of interpolation when upgrading (lower = faster transition)")]
    [Range(0.01f, 2f)]
    private float upgradeTransitionDuration = 0.5f;

    [SerializeField]
    [Tooltip("Easing function for upgrade transition")]
    private Ease upgradeEase = Ease.OutCubic;

    [Header("Intensity Mapping")]
    [SerializeField]
    [Tooltip("Intensity value for level 0 (base tower)")]
    [Range(0f, 1f)]
    private float baseIntensity = 0f;

    [SerializeField]
    [Tooltip("Intensity value per upgrade level (will be multiplied by level)")]
    [Range(0f, 1f)]
    private float intensityPerLevel = 0.3f;

    [SerializeField]
    [Tooltip("Maximum intensity cap (prevents values above 1)")]
    [Range(0f, 1f)]
    private float maxIntensity = 1f;

    private int intensityPropertyID;
    private Renderer[] allRenderers;
    private float currentIntensity;
    private MotionHandle motionHandle;

    private void Awake()
    {
        if (tower == null)
            tower = GetComponent<Tower>();

        if (renderers == null)
            renderers = transform;

        intensityPropertyID = Shader.PropertyToID(intensityPropertyName);

        CacheRenderers();
        currentIntensity = baseIntensity;
    }

    private void CacheRenderers()
    {
        if (renderers != null)
        {
            allRenderers = renderers.GetComponentsInChildren<Renderer>(true);
        }
        else
        {
            allRenderers = new Renderer[0];
        }
    }

    private void OnEnable()
    {
        if (tower != null)
            tower.OnUpgraded += OnTowerUpgraded;
    }

    private void OnDisable()
    {
        if (tower != null)
            tower.OnUpgraded -= OnTowerUpgraded;

        motionHandle.TryCancel();
    }

    private void Start()
    {
        UpdateShaderIntensity(CalculateIntensityForLevel(tower != null ? tower.CurrentLevel : 0), false);
    }

    private void OnTowerUpgraded(Tower upgradedTower)
    {
        float targetIntensity = CalculateIntensityForLevel(upgradedTower.CurrentLevel);
        UpdateShaderIntensity(targetIntensity, true);
    }

    private float CalculateIntensityForLevel(int level)
    {
        float intensity = baseIntensity + (intensityPerLevel * level);
        return Mathf.Clamp(intensity, 0f, maxIntensity);
    }

    private void UpdateShaderIntensity(float targetIntensity, bool animate)
    {
        if (allRenderers == null || allRenderers.Length == 0)
            return;

        motionHandle.TryCancel();

        if (animate && upgradeTransitionDuration > 0.01f)
        {
            motionHandle = LMotion
                .Create(currentIntensity, targetIntensity, upgradeTransitionDuration)
                .WithEase(upgradeEase)
                .Bind(value =>
                {
                    currentIntensity = value;
                    ApplyShaderIntensity(value);
                })
                .AddTo(gameObject);
        }
        else
        {
            currentIntensity = targetIntensity;
            ApplyShaderIntensity(targetIntensity);
        }
    }

    private void ApplyShaderIntensity(float intensity)
    {
        if (allRenderers == null || allRenderers.Length == 0)
            return;

        foreach (Renderer renderer in allRenderers)
        {
            if (renderer == null)
                continue;

            // Use material property block to avoid creating new material instances
            var propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(intensityPropertyID, intensity);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }

    public void SetIntensity(float intensity, bool animate = false)
    {
        float clampedIntensity = Mathf.Clamp(intensity, 0f, maxIntensity);
        UpdateShaderIntensity(clampedIntensity, animate);
    }

    private void Reset()
    {
        if (tower == null)
            tower = GetComponent<Tower>();

        if (renderers == null)
            renderers = transform;
    }
}
