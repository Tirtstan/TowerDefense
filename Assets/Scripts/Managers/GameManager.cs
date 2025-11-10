using System;
using System.Collections;
using LitMotion;
using QFSW.QC;
using UnityEngine;
using Random = UnityEngine.Random;

[CommandPrefix("game.")]
[DefaultExecutionOrder(-10)]
public class GameManager : Singleton<GameManager>
{
    public static event Action<GameState> OnGameStateChanged;
    public GameState CurrentGameState { get; private set; } = GameState.MainMenu;
    public float TimeSinceStart { get; private set; }
    private MotionHandle motionHandle;
    private float originalFogDensity;

    protected override void Awake()
    {
        base.Awake();
        Random.InitState((int)DateTime.Now.Ticks);
        originalFogDensity = RenderSettings.fogDensity;
    }

    private void Start()
    {
        RenderSettings.fogDensity = originalFogDensity / 4f;
        MainMenu();
    }

    private void Update()
    {
        if (CurrentGameState == GameState.Playing)
            TimeSinceStart += Time.deltaTime;
    }

    [Command("start_game", "Starts the game.")]
    public void StartGame()
    {
        if (CurrentGameState == GameState.Playing)
            return;

        EconomyManager.Instance.ResetCurrencyToDefault();
        ChangeGameState(GameState.Playing);

        motionHandle.TryCancel();
        motionHandle = LMotion
            .Create(RenderSettings.fogDensity, originalFogDensity, 2f)
            .WithEase(Ease.Linear)
            .Bind(value => RenderSettings.fogDensity = value)
            .AddTo(gameObject);

        Debug.Log("Game Started");
    }

    [Command("end_game", "Ends the game.")]
    public void EndGame()
    {
        ChangeGameState(GameState.GameOver);
        Debug.Log("Game Ended");
    }

    [Command("main_menu", "Returns to the main menu.")]
    public void MainMenu()
    {
        TimeSinceStart = 0f;
        ChangeGameState(GameState.MainMenu);

        motionHandle.TryCancel();
        motionHandle = LMotion
            .Create(RenderSettings.fogDensity, originalFogDensity / 4f, 2f)
            .WithEase(Ease.Linear)
            .Bind(value => RenderSettings.fogDensity = value)
            .AddTo(gameObject);
    }

    [Command("restart_game", "Restarts the game from the beginning.")]
    public void RestartGame()
    {
        // Change to MainMenu first to trigger cleanup (towers, waves, etc.)
        ChangeGameState(GameState.MainMenu);
        StartCoroutine(RestartCoroutine());
    }

    private IEnumerator RestartCoroutine()
    {
        yield return null;
        StartGame();
    }

    private void ChangeGameState(GameState newState)
    {
        CurrentGameState = newState;
        OnGameStateChanged?.Invoke(newState);
    }
}

public enum GameState
{
    MainMenu = 0,
    Playing = 1,
    GameOver = 2
}
