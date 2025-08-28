using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle)), DisallowMultipleComponent]
public class ToggleOnAction : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private InputActionReference inputAction;

    [SerializeField]
    [Range(1, 10)]
    private int actionScale = 1;
    private Toggle toggle;
    private PlayerInput playerInput;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        playerInput = PlayerInput.GetPlayerByIndex(0);
    }

    private void OnEnable()
    {
        playerInput.actions.FindAction(inputAction.action.id).performed += OnActionTriggered;
    }

    private void OnActionTriggered(InputAction.CallbackContext context)
    {
        int scale = (int)context.ReadValue<float>();
        if (actionScale == scale)
            toggle.isOn = !toggle.isOn;
    }

    private void OnDisable()
    {
        if (playerInput != null)
            playerInput.actions.FindAction(inputAction.action.id).performed -= OnActionTriggered;
    }
}
