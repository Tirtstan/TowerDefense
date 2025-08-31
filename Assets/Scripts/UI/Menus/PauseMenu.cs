using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static event Action<bool> OnPauseToggle;

    [Header("Components")]
    [SerializeField]
    private GameObject menu;

    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = PlayerInput.GetPlayerByIndex(0);
        playerInput.actions.FindAction("Player/Pause").performed += OnPausePerformed;

        Resume();
    }

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
