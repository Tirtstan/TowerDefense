using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private PlayerInput playerInput;

    [SerializeField]
    private CinemachineCamera cinemachineCamera;

    [SerializeField]
    private Transform target;

    [Header("Configs")]
    [Header("Movement")]
    [SerializeField]
    private float distance = 80f;

    [SerializeField]
    private Vector2 distanceClamp = new(60, 120);

    [SerializeField]
    private float distanceStep = 5f;

    [SerializeField]
    private float rotationSpeed = 100f;

    [Header("Damping")]
    [SerializeField]
    private float dampingStrength = 5f;

    [SerializeField]
    private float minimumVelocity = 0.01f;

    private Vector2 lookInput;
    private Vector2 panInput;
    private Vector2 currentVelocity;
    private bool isDragging;
    private float horizontalAngle;
    private float verticalAngle;

    private void Awake()
    {
        playerInput.actions.FindAction("Player/Look").performed += OnLook;
        playerInput.actions.FindAction("Player/Look").canceled += OnLook;

        playerInput.actions.FindAction("Player/Pan").performed += OnPan;
        playerInput.actions.FindAction("Player/Pan").canceled += OnPan;

        playerInput.actions.FindAction("Player/Drag").performed += OnDrag;
        playerInput.actions.FindAction("Player/Drag").canceled += OnDrag;

        playerInput.actions.FindAction("Player/Zoom").performed += OnZoomPerformed;
    }

    private void OnDrag(InputAction.CallbackContext context) => isDragging = context.ReadValueAsButton();

    private void OnPan(InputAction.CallbackContext context) => panInput = context.ReadValue<Vector2>();

    private void OnLook(InputAction.CallbackContext context) => lookInput = context.ReadValue<Vector2>();

    private void OnZoomPerformed(InputAction.CallbackContext context)
    {
        float zoomInput = -context.ReadValue<float>();
        distance += zoomInput * distanceStep;
        distance = Mathf.Clamp(distance, distanceClamp.x, distanceClamp.y);
    }

    private void Update()
    {
        float currentSensitivity = GetCurrentSensitivity();

        Vector2 activeInput = isDragging ? panInput : lookInput;
        Vector2 inputVelocity = currentSensitivity * rotationSpeed * activeInput;

        if (activeInput.magnitude > 0.01f)
        {
            currentVelocity = Vector2.Lerp(currentVelocity, inputVelocity, dampingStrength * Time.deltaTime);
        }
        else
        {
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, dampingStrength * Time.deltaTime);

            if (currentVelocity.magnitude < minimumVelocity)
                currentVelocity = Vector2.zero;
        }

        float horizontalInput = currentVelocity.x * Time.deltaTime;
        float verticalInput = -currentVelocity.y * Time.deltaTime;

        horizontalAngle += horizontalInput;
        verticalAngle += verticalInput;

        verticalAngle = Mathf.Clamp(verticalAngle, -89f, 89f);

        Quaternion rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0);
        Vector3 direction = rotation * Vector3.forward;

        cinemachineCamera.transform.position = target.position - direction * distance;
        cinemachineCamera.transform.LookAt(target);
    }

    private float GetCurrentSensitivity()
    {
        ControlsSettings controlsSettings = OptionsManager.Instance.Settings.Controls;

        return playerInput.currentControlScheme switch
        {
            "Gamepad" => controlsSettings.GamepadLookSensitivity,
            "Keyboard&Mouse" => isDragging ? controlsSettings.MouseDragSensitivity : controlsSettings.KeyNavSensitivity,
            _ => 1f,
        };
    }

    private void OnDestroy()
    {
        if (playerInput == null)
            return;

        playerInput.actions.FindAction("Player/Look").performed -= OnLook;
        playerInput.actions.FindAction("Player/Look").canceled -= OnLook;

        playerInput.actions.FindAction("Player/Pan").performed -= OnPan;
        playerInput.actions.FindAction("Player/Pan").canceled -= OnPan;

        playerInput.actions.FindAction("Player/Drag").performed -= OnDrag;
        playerInput.actions.FindAction("Player/Drag").canceled -= OnDrag;

        playerInput.actions.FindAction("Player/Zoom").performed -= OnZoomPerformed;
    }
}
