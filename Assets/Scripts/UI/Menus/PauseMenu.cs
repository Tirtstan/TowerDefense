using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static event Action<bool> OnPauseToggle;

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

        resumeButton.onClick.AddListener(Resume);
        restartButton.onClick.AddListener(Restart);
        optionsButton.onClick.AddListener(OpenOptions);
        mainMenuButton.onClick.AddListener(OpenMainMenu);
        exitButton.onClick.AddListener(Exit);

        Resume();
    }

    private void Exit() => Application.Quit();

    private void OpenMainMenu() => Debug.Log("Open Main Menu");

    private void OpenOptions() => Debug.Log("Open Options");

    private void Restart() => SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);

    private void OnPausePerformed(InputAction.CallbackContext context) => ToggleMenu();

    private void ToggleMenu()
    {
        if (menu.activeSelf)
        {
            Resume();
        }
        else
        {
            Pause();
        }

        OnPauseToggle?.Invoke(menu.activeSelf);
    }

    private void Resume()
    {
        menu.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Pause()
    {
        menu.SetActive(true);
        Time.timeScale = 0f;
    }

    private void OnDestroy()
    {
        if (playerInput != null)
            playerInput.actions.FindAction("Player/Pause").performed -= OnPausePerformed;
    }
}
