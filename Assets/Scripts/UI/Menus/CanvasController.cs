using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

public class CanvasController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private CanvasGroup mainMenuCanvas;

    [SerializeField]
    private CanvasGroup gameplayCanvas;

    private void Awake()
    {
        GameManager.OnGameStateChanged += OnGameStateChanged;

        mainMenuCanvas.alpha = 1f;
        mainMenuCanvas.interactable = true;
        mainMenuCanvas.blocksRaycasts = true;

        gameplayCanvas.alpha = 0f;
        gameplayCanvas.interactable = false;
        gameplayCanvas.blocksRaycasts = false;
    }

    private void OnGameStateChanged(GameState newState)
    {
        UpdateCanvases(newState);
    }

    private void UpdateCanvases(GameState gameState)
    {
        bool showMainMenu = gameState == GameState.MainMenu;
        bool showGameplay = gameState == GameState.Playing;

        mainMenuCanvas.interactable = showMainMenu;
        mainMenuCanvas.blocksRaycasts = showMainMenu;

        gameplayCanvas.interactable = showGameplay;
        gameplayCanvas.blocksRaycasts = showGameplay;

        LSequence
            .Create()
            .Append(
                LMotion
                    .Create(mainMenuCanvas.alpha, showMainMenu ? 1f : 0f, 1f)
                    .WithEase(Ease.OutQuad)
                    .BindToAlpha(mainMenuCanvas)
                    .AddTo(gameObject)
            )
            .Append(
                LMotion
                    .Create(gameplayCanvas.alpha, showGameplay ? 1f : 0f, 1f)
                    .WithDelay(0.5f)
                    .WithEase(Ease.OutQuad)
                    .BindToAlpha(gameplayCanvas)
                    .AddTo(gameObject)
            )
            .Run();
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
}
