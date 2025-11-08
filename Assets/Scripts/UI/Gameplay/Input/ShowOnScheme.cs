using UnityEngine;
using UnityEngine.InputSystem;

public class ShowOnScheme : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject keyboardMousePanel;

    [SerializeField]
    private GameObject gamepadPanel;
    private PlayerInput playerInput;
    private const string GamepadScheme = "Gamepad";

    private void Awake()
    {
        playerInput = PlayerInput.GetPlayerByIndex(0);
        playerInput.onControlsChanged += OnControlsChanged;
        OnControlsChanged(playerInput);
    }

    private void OnControlsChanged(PlayerInput input)
    {
        if (input.currentControlScheme == GamepadScheme)
        {
            gamepadPanel.SetActive(true);
            keyboardMousePanel.SetActive(false);
        }
        else
        {
            gamepadPanel.SetActive(false);
            keyboardMousePanel.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        if (playerInput != null)
            playerInput.onControlsChanged -= OnControlsChanged;
    }
}
