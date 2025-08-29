using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SelectionSystem : Singleton<SelectionSystem>
{
    public static event Action<IGameSelectable> OnSelected;
    public static event Action<IGameSelectable> OnDeselected;

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
    private bool isOverUI;

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
        playerInput = PlayerInput.GetPlayerByIndex(0);

        playerInput.actions.FindAction("MousePosition").performed += OnMousePosition;
        playerInput.actions.FindAction("MousePosition").canceled += OnMousePosition;
        playerInput.actions.FindAction("Place").performed += OnSelectPerformed;
    }

    private void Update()
    {
        isOverUI = EventSystem.current.IsPointerOverGameObject();
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
                {
                    // interface causing weird null issues
                    // if (currentSelected is MonoBehaviour mb && mb != null)
                    if (currentSelected.Transform != null)
                        currentSelected.Deselect();
                }

                SelectObject(selectable);
                Debug.Log($"Selected: {selectable}");
            }
        }
        else if (!isOverUI)
        {
            DeselectAll();
        }
    }

    private void DeselectAll()
    {
        if (currentSelected != null)
        {
            // interface causing weird null issues
            // if (currentSelected is MonoBehaviour mb && mb != null)
            if (currentSelected.Transform != null)
            {
                currentSelected.Deselect();
                OnDeselected?.Invoke(currentSelected);
            }

            currentSelected = null;
        }
    }

    public void SelectObject(IGameSelectable selectable)
    {
        currentSelected = selectable;
        selectable.Select();
        OnSelected?.Invoke(selectable);
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
