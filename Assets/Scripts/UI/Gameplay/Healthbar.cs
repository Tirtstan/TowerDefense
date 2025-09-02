using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class Healthbar : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Image fillImage;

    [Header("Configs")]
    [SerializeField]
    private Vector2 offset = new(0, 50f);

    private HealthbarController controller;
    private Transform target;
    private IDamagable damagable;
    private float timer;
    private float displayDuration;
    private Camera mainCamera;
    private Canvas canvas;
    private RectTransform rectTransform;
    private bool isReleasing;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Init(
        HealthbarController controller,
        Transform target,
        IDamagable damagable,
        float displayDuration,
        Camera camera,
        Canvas canvas
    )
    {
        this.controller = controller;
        this.target = target;
        this.damagable = damagable;
        this.displayDuration = displayDuration;
        mainCamera = camera;
        this.canvas = canvas;

        timer = displayDuration;
        isReleasing = false;

        damagable.OnHealthChanged += UpdateHealthbar;
        damagable.OnDeath += OnTargetDeath;

        float max = damagable.MaxHealth;
        fillImage.fillAmount = max <= 0f ? 0f : damagable.CurrentHealth / max;

        UpdatePosition();
    }

    private void Update()
    {
        if (isReleasing)
            return;

        if (target == null || damagable == null)
        {
            ReleaseSelf();
            return;
        }

        timer -= Time.deltaTime;
        UpdatePosition();

        if (timer <= 0f)
            ReleaseSelf();
    }

    private void UpdatePosition()
    {
        if (target == null || mainCamera == null || canvas == null)
            return;

        Vector3 screenPoint = mainCamera.WorldToScreenPoint(target.position);

        if (screenPoint.z < 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        screenPoint += (Vector3)offset;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPoint,
            null,
            out Vector2 localPoint
        );

        rectTransform.anchoredPosition = localPoint;
    }

    public void ResetTimer()
    {
        if (!isReleasing)
            timer = displayDuration;
    }

    private void UpdateHealthbar(IDamagable damagable)
    {
        if (isReleasing)
            return;

        fillImage.fillAmount = damagable.CurrentHealth / damagable.MaxHealth;
        if (damagable.CurrentHealth <= 0)
            ReleaseSelf();
    }

    private void OnTargetDeath() => ReleaseSelf();

    private void ReleaseSelf()
    {
        if (isReleasing)
            return;

        isReleasing = true;

        if (controller != null && damagable != null)
            controller.ReleaseHealthbar(this, damagable);
    }

    private void OnDisable()
    {
        if (damagable != null)
        {
            damagable.OnHealthChanged -= UpdateHealthbar;
            damagable.OnDeath -= OnTargetDeath;
        }

        controller = null;
        target = null;
        damagable = null;
        mainCamera = null;
        canvas = null;
        timer = 0f;
        isReleasing = false;
    }
}
