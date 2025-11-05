using UnityEngine;
using UnityEngine.SceneManagement;

public class GenerateMap : MonoBehaviour
{
    [Header("Generator")]
    [SerializeField]
    private Generator generator;
    private bool hasGeneratedThisSession;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        generator.Generate();
        hasGeneratedThisSession = true;
    }

    private void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Playing:
                // Generate map if player hasn't generated one yet this session
                if (!hasGeneratedThisSession)
                {
                    generator.Generate();
                    hasGeneratedThisSession = true;
                }
                break;

            case GameState.MainMenu:
            case GameState.GameOver:
                hasGeneratedThisSession = false;
                break;
        }
    }

    private void Reset()
    {
        generator = GetComponent<Generator>();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
}
