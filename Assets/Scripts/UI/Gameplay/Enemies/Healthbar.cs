using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Image fillImage;

    [Header("Configs")]
    [SerializeField]
    private Vector2 offset = new(0, 1f);
    private EnemyHealthbarController controller;
    private Transform target;
    private IDamagable damagable;
    private float timer;
    private float displayDuration;

    private void Awake()
    {
        UpdatePosition();
    }

    public void Init(EnemyHealthbarController controller, Transform target, IDamagable damagable, float displayDuration)
    {
        this.controller = controller;
        this.target = target;
        this.damagable = damagable;
        this.displayDuration = displayDuration;
        timer = displayDuration;

        damagable.OnHealthChanged += UpdateHealthbar;

        float max = damagable.MaxHealth;
        fillImage.fillAmount = max <= 0f ? 0f : damagable.CurrentHealth / max;
    }

    private void Update() => UpdatePosition();

    private void UpdatePosition()
    {
        if (target != null)
            transform.position = target.position + (Vector3)offset;
    }

    public void ResetTimer() => timer = displayDuration;

    public bool TryUpdateTimer(float deltaTime)
    {
        timer -= deltaTime;
        return timer <= 0f;
    }

    private void UpdateHealthbar(IDamagable damagable)
    {
        fillImage.fillAmount = damagable.CurrentHealth / damagable.MaxHealth;

        if (damagable.CurrentHealth <= 0)
            controller.ReleaseHealthbar(this);
    }

    private void OnDisable()
    {
        if (damagable != null)
            damagable.OnHealthChanged -= UpdateHealthbar;

        controller = null;
        target = null;
        damagable = null;
        timer = 0f;
    }
}
