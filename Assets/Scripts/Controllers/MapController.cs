using UnityEngine;
using UnityEngine.InputSystem;

public class MapController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private PlayerInput playerInput;

    [SerializeField]
    private Transform mapRoot;

    [SerializeField]
    private Transform cameraTransform;

    [Header("Configs")]
    [Header("Rotation")]
    [SerializeField]
    private float rotationSpeed = 100f;

    [Header("Zoom")]
    [SerializeField]
    private Vector2 zoomClamp = new(14, 30);

    [SerializeField]
    [Range(1, 10)]
    private float zoomStep = 1f;

    [SerializeField]
    private float zoomRate = 20f;

    [Header("Damping")]
    [SerializeField]
    private float dampingStrength = 15f;

    [SerializeField]
    private float minimumVelocity = 0.01f;

    private Vector2 lookInput;
    private Vector2 panInput;
    private Vector2 currentVelocity;
    private bool isDragging;
    private float distance = 20f;
    private float targetDistance;
    private Vector3 cameraRightAxis;
    private Vector3 cameraUpAxis;

    private void Awake()
    {
        if (mapRoot == null)
            mapRoot = transform;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        CalculateRotationAxes();
        targetDistance = distance;
        UpdateCameraPosition();

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
        targetDistance += zoomInput * zoomStep;
        targetDistance = Mathf.Clamp(targetDistance, zoomClamp.x, zoomClamp.y);
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

        if (currentVelocity.magnitude > 0.01f && cameraTransform != null)
        {
            float horizontalInput = currentVelocity.x * Time.deltaTime;
            float verticalInput = currentVelocity.y * Time.deltaTime;

            mapRoot.Rotate(cameraUpAxis, -horizontalInput, Space.World);
            mapRoot.Rotate(cameraRightAxis, verticalInput, Space.World);
        }

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        if (cameraTransform != null)
        {
            distance = Mathf.Lerp(distance, targetDistance, zoomRate * Time.deltaTime);

            Vector3 direction = cameraTransform.forward;
            Vector3 targetPosition = mapRoot.position - direction * distance;
            cameraTransform.position = targetPosition;
        }
    }

    private void CalculateRotationAxes()
    {
        if (cameraTransform == null)
            return;

        cameraRightAxis = cameraTransform.right;
        cameraUpAxis = cameraTransform.up;

        Vector3 cameraForward = cameraTransform.forward;
        cameraRightAxis = Vector3.Cross(cameraUpAxis, cameraForward).normalized;
        cameraUpAxis = Vector3.Cross(cameraForward, cameraRightAxis).normalized;
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
