using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(-10)]
public class GameManager : Singleton<GameManager>
{
    public static event Action OnGameStart;
    public static event Action OnGameEnd;
    public bool HasGameStarted { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Random.InitState((int)DateTime.Now.Ticks);
    }

    private void Update()
    {
        if (Keyboard.current.f5Key.wasPressedThisFrame)
        {
            StartGame();
        }

        if (Keyboard.current.f6Key.wasPressedThisFrame)
        {
            EndGame();
        }
    }

    public void StartGame()
    {
        if (HasGameStarted)
            return;

        OnGameStart?.Invoke();
        HasGameStarted = true;

        Debug.Log("Game Started");
    }

    public void EndGame()
    {
        OnGameEnd?.Invoke();
        HasGameStarted = false;

        Debug.Log("Game Ended");
    }
}
