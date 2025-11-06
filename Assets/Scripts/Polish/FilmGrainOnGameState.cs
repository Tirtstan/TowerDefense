using LitMotion;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FilmGrainOnGameState : MonoBehaviour
{
    [Header("Volume")]
    [SerializeField]
    private Volume volume;

    [Header("Film Grain")]
    [SerializeField]
    [Range(0f, 1f)]
    private float defaultIntensity = 0.4f;

    [SerializeField]
    private float transitionDuration = 1f;
    private FilmGrain filmGrain;

    private void Awake()
    {
        volume.profile.TryGet(out filmGrain);
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                FilmGrainTransition(0f);
                break;
            case GameState.MainMenu:
            case GameState.GameOver:
                FilmGrainTransition(defaultIntensity);
                break;
        }
    }

    private MotionHandle FilmGrainTransition(float to)
    {
        return LMotion
            .Create(filmGrain.intensity.value, to, transitionDuration)
            .Bind(filmGrain, (value, filmGrain) => filmGrain.intensity.value = value)
            .AddTo(gameObject);
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
}
