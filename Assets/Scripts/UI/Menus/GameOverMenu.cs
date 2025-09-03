using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private GameObject menu;

    [SerializeField]
    private Button restartButton;

    [SerializeField]
    private Button mainMenuButton;

    [SerializeField]
    private Button exitButton;

    private void Awake()
    {
        HideMenu();
        GameManager.OnGameEnd += ShowMenu;

        restartButton.onClick.AddListener(OnRestartClicked);
    }

    private void OnRestartClicked() => SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);

    private void OnMainMenuClicked() => Debug.Log("Main Menu Button Clicked");

    private void OnExitClicked() => Application.Quit();

    private void ShowMenu() => menu.SetActive(true);

    private void HideMenu() => menu.SetActive(false);

    private void OnDestroy()
    {
        GameManager.OnGameEnd -= ShowMenu;
    }
}
