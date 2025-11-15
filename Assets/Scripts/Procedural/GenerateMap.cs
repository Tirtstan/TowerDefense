using UnityEngine;

public class GenerateMap : MonoBehaviour
{
    [Header("Generator")]
    [SerializeField]
    private Generator generator;
    private bool isMapDirty;

    private void Start()
    {
        generator.Generate();
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
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
}
