using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(-10)]
public class GameManager : Singleton<GameManager>
{
    public static event Action OnGameStart;
    public static event Action OnGameEnd;

    [Header("Configs")]
    [SerializeField]
    private float timeBeforeStart = 5f;
    public bool HasGameStarted { get; private set; }
    public float TimeSinceStart { get; private set; }
    private WaitForSeconds waitForStart;

    protected override void Awake()
    {
        base.Awake();
        Random.InitState((int)DateTime.Now.Ticks);
        waitForStart = new WaitForSeconds(timeBeforeStart);
    }

    private void Start()
    {
        StartCoroutine(StartGameAfterDelay());
    }

    private IEnumerator StartGameAfterDelay()
    {
        yield return waitForStart;
        StartGame();
    }

    private void Update()
    {
        if (HasGameStarted)
            TimeSinceStart += Time.deltaTime;

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
