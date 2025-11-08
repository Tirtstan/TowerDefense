using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private CinemachineCamera gameplayCamera;

    [SerializeField]
    private CinemachineCamera mainMenuCamera;

    private void Awake()
    {
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                mainMenuCamera.Priority = 10;
                gameplayCamera.Priority = -10;
                break;
            case GameState.Playing:
                mainMenuCamera.Priority = -10;
                gameplayCamera.Priority = 10;
                break;
        }
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
}
