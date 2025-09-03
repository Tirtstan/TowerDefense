using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : Singleton<PauseManager>
{
    public static event Action<bool> OnPauseToggle;
    public bool IsPaused { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Resume();

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        OnPauseToggle?.Invoke(IsPaused);
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        OnPauseToggle?.Invoke(IsPaused);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
