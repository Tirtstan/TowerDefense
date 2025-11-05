using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

public class SpinOnGameStart : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private float spinSpeed = 90f;

    private Quaternion originalRotation;
    private MotionHandle currentMotion;

    private void Awake()
    {
        originalRotation = transform.rotation;
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState gameState)
    {
        if (gameState == GameState.Playing)
        {
            currentMotion.TryCancel();
            currentMotion = LMotion
                .Create(transform.rotation, originalRotation, 1f)
                .WithEase(Ease.OutQuart)
                .BindToRotation(transform)
                .AddTo(gameObject);
        }
        else if (gameState == GameState.GameOver)
        {
            currentMotion.TryCancel();
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameState != GameState.Playing)
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
}
