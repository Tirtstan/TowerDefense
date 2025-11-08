using UnityEngine;
using UnityEngine.SceneManagement;

public class GenerateMap : MonoBehaviour
{
    [Header("Generator")]
    [SerializeField]
    private Generator generator;
    private bool isMapDirty;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => generator.Generate();

    private void Start()
    {
        GameManager.OnGameStateChanged += OnGameStateChanged; // prevent first load marking map as dirty
        isMapDirty = false;
    }

    private void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Playing:
            {
                if (isMapDirty) // Generate map if player hasn't generated one yet this session
                {
                    generator.Generate();
                    isMapDirty = false;
                }
                break;
            }

            case GameState.MainMenu
            or GameState.GameOver:
                isMapDirty = true;
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
