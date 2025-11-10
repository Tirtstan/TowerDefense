using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private GameObject menu;

    [SerializeField]
    private TextMeshProUGUI infoText;

    [SerializeField]
    private Button restartButton;

    [SerializeField]
    private Button optionsButton;

    [SerializeField]
    private Button mainMenuButton;

    [SerializeField]
    private Button exitButton;

    private void Awake()
    {
        HideMenu();
        GameManager.OnGameStateChanged += OnGameStateChanged;

        restartButton.onClick.AddListener(OnRestartClicked);
        optionsButton.onClick.AddListener(OnOptionsClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnGameStateChanged(GameState gameState)
    {
        if (gameState != GameState.GameOver)
        {
            HideMenu();
            return;
        }

        infoText.SetText(FormatUtils.FormatTime(GameManager.Instance.TimeSinceStart));
        ShowMenu();
    }

    private void OnRestartClicked() => GameManager.Instance.RestartGame();

    private void OnOptionsClicked() => Debug.Log("Options Button Clicked");

    private void OnMainMenuClicked()
    {
        GameManager.Instance.MainMenu();
        HideMenu();
    }

    private void OnExitClicked() => Application.Quit();

    private void ShowMenu() => menu.SetActive(true);

    private void HideMenu() => menu.SetActive(false);

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
}
