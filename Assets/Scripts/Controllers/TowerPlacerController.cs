using System;
using Riten.Native.Cursors;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TowerPlacerController : Singleton<TowerPlacerController>
{
    public static event Action<TowerSO> OnTowerSelected;
    public static event Action<TowerSO> OnTowerDeselected;
    public static event Action<Tower> OnTowerPlaced;

    [Header("Configs")]
    [SerializeField]
    private NTCursors placingCursor = NTCursors.ResizeVertical;

    [SerializeField]
    private float maxPlacementDistance = 50f;

    [SerializeField]
    private LayerMask groundLayer;
    private PlayerInput playerInput;
    private int cursorId;
    private TowerSO currentTowerSO;
    private Tower previewTower;
    private Camera mainCamera;
    private Vector2 mousePosition;
    private static bool isOverUI;

    protected override void Awake()
    {
        base.Awake();
        playerInput = PlayerInput.GetPlayerByIndex(0);
        mainCamera = Camera.main;

        playerInput.actions.FindAction("MousePosition").performed += OnMousePosition;
        playerInput.actions.FindAction("MousePosition").canceled += OnMousePosition;
        playerInput.actions.FindAction("Place").performed += OnPlacePerformed;
        playerInput.actions.FindAction("Drag").performed += OnDragPerformed;
    }

    private void Update()
    {
        isOverUI = EventSystem.current.IsPointerOverGameObject();

        if (previewTower != null)
        {
            if (Raycast(out RaycastHit hit))
                previewTower.transform.position = hit.point;
        }
    }

    private void OnPlacePerformed(InputAction.CallbackContext context)
    {
        if (currentTowerSO == null)
            return;

        if (Raycast(out RaycastHit hit))
            PlaceTower(hit.point);
    }

    private bool Raycast(out RaycastHit hit)
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        return Physics.Raycast(ray, out hit, maxPlacementDistance, groundLayer);
    }

    private void OnMousePosition(InputAction.CallbackContext context) => mousePosition = context.ReadValue<Vector2>();

    private void OnDragPerformed(InputAction.CallbackContext context) => DeselectCurrentTower();

    public void DeselectCurrentTower()
    {
        if (currentTowerSO == null)
            return;

        OnTowerDeselected?.Invoke(currentTowerSO);

        if (previewTower != null)
            Destroy(previewTower.gameObject);

        currentTowerSO = null;
        previewTower = null;
        CursorStack.Pop(cursorId);
    }

    public bool TryDeselectTower(TowerSO tower)
    {
        if (currentTowerSO != tower)
            return false;

        DeselectCurrentTower();
        return true;
    }

    public void SetCurrentTower(TowerSO tower)
    {
        currentTowerSO = tower;
        previewTower = Instantiate(currentTowerSO.Prefab);
        OnTowerSelected?.Invoke(currentTowerSO);

        cursorId = CursorStack.Push(placingCursor);
    }

    public TowerSO GetCurrentTower() => currentTowerSO;

    public void PlaceTower(Vector3 position)
    {
        if (currentTowerSO == null)
            return;

        if (isOverUI)
            return;

        Tower placedTower = Instantiate(currentTowerSO.Prefab);
        placedTower.transform.position = position;
        OnTowerPlaced?.Invoke(placedTower);
    }

    private void OnDestroy()
    {
        if (playerInput == null)
            return;

        playerInput.actions.FindAction("MousePosition").performed -= OnMousePosition;
        playerInput.actions.FindAction("MousePosition").canceled -= OnMousePosition;
        playerInput.actions.FindAction("Place").performed -= OnPlacePerformed;
        playerInput.actions.FindAction("Drag").performed -= OnDragPerformed;
    }
}
