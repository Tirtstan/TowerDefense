using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private GameObject menu;

    [SerializeField]
    private Button resumeButton;

    [SerializeField]
    private Button restartButton;

    [SerializeField]
    private Button optionsButton;

    [SerializeField]
    private Button mainMenuButton;

    [SerializeField]
    private Button exitButton;
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = PlayerInput.GetPlayerByIndex(0);
        playerInput.actions.FindAction("Player/Pause").performed += OnPausePerformed;

        resumeButton.onClick.AddListener(OnResumeClicked);
        restartButton.onClick.AddListener(Restart);
        optionsButton.onClick.AddListener(OpenOptions);
        mainMenuButton.onClick.AddListener(OpenMainMenu);
        exitButton.onClick.AddListener(Exit);

        OnResumeClicked();
    }

    private void Restart() => SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);

    private void OpenMainMenu() => Debug.Log("Open Main Menu");

    private void OpenOptions() => Debug.Log("Open Options");

    private void Exit() => Application.Quit();

    private void OnPausePerformed(InputAction.CallbackContext context) => ToggleMenu();

    private void ToggleMenu()
    {
        if (menu.activeSelf)
        {
            OnResumeClicked();
        }
        else
        {
            OnPauseClicked();
        }
    }

    private void OnResumeClicked()
    {
        menu.SetActive(false);
        PauseManager.Instance.Resume();
    }

    private void OnPauseClicked()
    {
        menu.SetActive(true);
        PauseManager.Instance.Pause();
    }

    private void OnDestroy()
    {
        if (playerInput != null)
            playerInput.actions.FindAction("Player/Pause").performed -= OnPausePerformed;
    }
}
