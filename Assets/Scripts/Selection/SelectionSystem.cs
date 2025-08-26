using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectionSystem : Singleton<SelectionSystem>
{
    public static event Action<IGameSelectable> OnSelected;
    public static event Action OnDeselected;

    [Header("Selection")]
    [Range(10, 200)]
    [SerializeField]
    private float selectionDistance = 100f;

    [SerializeField]
    private LayerMask selectionLayer;

    private IGameSelectable currentSelected;
    private PlayerInput playerInput;
    private Camera mainCamera;
    private Vector2 mousePosition;

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
        playerInput = PlayerInput.GetPlayerByIndex(0);

        playerInput.actions.FindAction("MousePosition").performed += OnMousePosition;
        playerInput.actions.FindAction("MousePosition").canceled += OnMousePosition;
        playerInput.actions.FindAction("Place").performed += OnSelectPerformed;
    }

    private void OnMousePosition(InputAction.CallbackContext context) => mousePosition = context.ReadValue<Vector2>();

    private void OnSelectPerformed(InputAction.CallbackContext context)
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, selectionDistance, selectionLayer))
        {
            if (hit.collider.TryGetComponent(out IGameSelectable selectable))
            {
                if (currentSelected == selectable)
                    return;

                // Deselect current if different
                if (currentSelected != null && currentSelected != selectable)
                    currentSelected.Deselect();

                // Select new
                currentSelected = selectable;
                selectable.Select();
                OnSelected?.Invoke(selectable);
            }
        }
        else
        {
            DeselectAll();
        }
    }

    private void DeselectAll()
    {
        if (currentSelected != null)
        {
            currentSelected.Deselect();
            currentSelected = null;
            OnDeselected?.Invoke();
        }
    }

    private void OnDestroy()
    {
        if (playerInput == null)
            return;

        playerInput.actions.FindAction("MousePosition").performed -= OnMousePosition;
        playerInput.actions.FindAction("MousePosition").canceled -= OnMousePosition;
        playerInput.actions.FindAction("Place").performed -= OnSelectPerformed;
    }
}
