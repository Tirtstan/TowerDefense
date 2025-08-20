using System;
using Riten.Native.Cursors;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlatMapController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private PlayerInput playerInput;

    [SerializeField]
    private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 10f;

    [SerializeField]
    private Vector2 mapBounds = new(50f, 50f);

    [Header("Rotation")]
    [SerializeField]
    private float rotationSpeed = 100f;

    [Header("Zoom")]
    [SerializeField]
    private Vector2 zoomClamp = new(5, 25);

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

    private Vector2 moveInput;
    private Vector2 panInput;
    private float rotateInput;
    private Vector2 currentVelocity;
    private Vector3 cameraPosition;
    private bool isDragging;
    private float currentRotation;
    private float distance;
    private float targetDistance;

    private void Awake()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        targetDistance = (zoomClamp.x + zoomClamp.y) / 2f;
        cameraPosition = cameraTransform.position;
        currentRotation = cameraTransform.eulerAngles.y;

        UpdateCameraPosition();

        playerInput.actions.FindAction("Player/Look").performed += OnMove;
        playerInput.actions.FindAction("Player/Look").canceled += OnMove;

        playerInput.actions.FindAction("Player/Pan").performed += OnPan;
        playerInput.actions.FindAction("Player/Pan").canceled += OnPan;

        playerInput.actions.FindAction("Player/Drag").performed += OnDrag;
        playerInput.actions.FindAction("Player/Drag").canceled += OnDrag;

        playerInput.actions.FindAction("Player/Zoom").performed += OnZoomPerformed;

        playerInput.actions.FindAction("Player/Rotate").performed += OnRotate;
        playerInput.actions.FindAction("Player/Rotate").canceled += OnRotate;
    }

    private void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();

    private void OnPan(InputAction.CallbackContext context)
    {
        panInput = context.ReadValue<Vector2>();

        if (context.performed && isDragging)
            CursorStack.Push(NTCursors.ClosedHand);
        else
            CursorStack.Pop();
    }

    private void OnDrag(InputAction.CallbackContext context)
    {
        isDragging = context.ReadValueAsButton();

        if (context.performed)
            CursorStack.Push(NTCursors.OpenHand);
        else
            CursorStack.Clear();
    }

    private void OnRotate(InputAction.CallbackContext context) => rotateInput = context.ReadValue<float>();

    private void OnZoomPerformed(InputAction.CallbackContext context)
    {
        float zoomInput = -context.ReadValue<float>();
        targetDistance += zoomInput * zoomStep;
        targetDistance = Mathf.Clamp(targetDistance, zoomClamp.x, zoomClamp.y);
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        UpdateCameraPosition();
    }

    private void HandleMovement()
    {
        Vector2 inputVelocity = Vector2.zero;

        if (moveInput.magnitude > 0.01f)
            inputVelocity = moveInput * moveSpeed;

        if (isDragging && panInput.magnitude > 0.01f)
            inputVelocity = GetCurrentSensitivity() * -panInput;

        if (inputVelocity.magnitude > 0.01f)
        {
            currentVelocity = Vector2.Lerp(currentVelocity, inputVelocity, dampingStrength * Time.deltaTime);
        }
        else
        {
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, dampingStrength * Time.deltaTime);

            if (currentVelocity.magnitude < minimumVelocity)
                currentVelocity = Vector2.zero;
        }

        if (currentVelocity.magnitude > 0.01f)
        {
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 worldMovement = cameraRight * currentVelocity.x + cameraForward * currentVelocity.y;
            cameraPosition += worldMovement * Time.deltaTime;

            cameraPosition.x = Mathf.Clamp(cameraPosition.x, -mapBounds.x, mapBounds.x);
            cameraPosition.z = Mathf.Clamp(cameraPosition.z, -mapBounds.y, mapBounds.y);
        }
    }

    private void HandleRotation()
    {
        if (Mathf.Abs(rotateInput) > 0.01f)
            currentRotation += rotateInput * rotationSpeed * Time.deltaTime;
    }

    private void UpdateCameraPosition()
    {
        if (cameraTransform == null)
            return;

        distance = Mathf.Lerp(distance, targetDistance, zoomRate * Time.deltaTime);
        Vector3 offset = new(0, distance * 0.7f, -distance * 0.7f);

        Quaternion rotation = Quaternion.Euler(0, currentRotation, 0);
        Vector3 rotatedOffset = rotation * offset;

        cameraTransform.position = cameraPosition + rotatedOffset;
        cameraTransform.LookAt(cameraPosition);
    }

    private float GetCurrentSensitivity()
    {
        ControlsSettings controlsSettings = OptionsManager.Instance.Settings.Controls;

        return playerInput.currentControlScheme switch
        {
            "Gamepad" => controlsSettings.GamepadLookSensitivity,
            "Keyboard&Mouse"
                => isDragging ? controlsSettings.MouseDragSensitivity * 5f : controlsSettings.KeyNavSensitivity,
            _ => 1f,
        };
    }

    private void OnDestroy()
    {
        if (playerInput == null)
            return;

        playerInput.actions.FindAction("Player/Look").performed -= OnMove;
        playerInput.actions.FindAction("Player/Look").canceled -= OnMove;

        playerInput.actions.FindAction("Player/Pan").performed -= OnPan;
        playerInput.actions.FindAction("Player/Pan").canceled -= OnPan;

        playerInput.actions.FindAction("Player/Drag").performed -= OnDrag;
        playerInput.actions.FindAction("Player/Drag").canceled -= OnDrag;

        playerInput.actions.FindAction("Player/Zoom").performed -= OnZoomPerformed;

        playerInput.actions.FindAction("Player/Rotate").performed -= OnRotate;
        playerInput.actions.FindAction("Player/Rotate").canceled -= OnRotate;
    }
}
