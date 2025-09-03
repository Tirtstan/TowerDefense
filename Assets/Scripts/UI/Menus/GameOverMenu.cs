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
        GameManager.OnGameEnd += OnGameEnd;

        restartButton.onClick.AddListener(OnRestartClicked);
        optionsButton.onClick.AddListener(OnOptionsClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnGameEnd()
    {
        PauseManager.Instance.Pause(); // probably do a different solution? (do this to prevent enemies trying to get center tower pos, null ref)
        infoText.SetText(FormatUtils.FormatTime(GameManager.Instance.TimeSinceStart));
        ShowMenu();
    }

    private void OnRestartClicked() => SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);

    private void OnOptionsClicked() => Debug.Log("Options Button Clicked");

    private void OnMainMenuClicked() => Debug.Log("Main Menu Button Clicked");

    private void OnExitClicked() => Application.Quit();

    private void ShowMenu() => menu.SetActive(true);

    private void HideMenu() => menu.SetActive(false);

    private void OnDestroy()
    {
        GameManager.OnGameEnd -= OnGameEnd;
    }
}
