using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TowerPlacer : Singleton<TowerPlacer>
{
    [Header("Configs")]
    [SerializeField]
    private float maxPlacementDistance = 50f;

    [SerializeField]
    private LayerMask groundLayer;
    public static event Action<TowerSO> OnTowerSelected;
    public static event Action<TowerSO> OnTowerDeselected;
    public static event Action<Tower> OnTowerPlaced;
    private PlayerInput playerInput;
    private TowerSO currentTower;
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
    }

    private void OnPlacePerformed(InputAction.CallbackContext context)
    {
        if (currentTower == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxPlacementDistance, groundLayer))
        {
            Vector3 worldPosition = hit.point;
            PlaceTower(worldPosition);
        }
    }

    private void OnMousePosition(InputAction.CallbackContext context) => mousePosition = context.ReadValue<Vector2>();

    private void OnDragPerformed(InputAction.CallbackContext context)
    {
        if (currentTower != null)
            DeselectCurrentTower();
    }

    public void DeselectCurrentTower()
    {
        OnTowerDeselected?.Invoke(currentTower);
        currentTower = null;
    }

    public bool TryDeselectTower(TowerSO tower)
    {
        if (currentTower != tower)
            return false;

        DeselectCurrentTower();
        return true;
    }

    public void SetCurrentTower(TowerSO tower)
    {
        currentTower = tower;
        OnTowerSelected?.Invoke(tower);
    }

    public TowerSO GetCurrentTower() => currentTower;

    public void PlaceTower(Vector3 position)
    {
        if (currentTower == null)
            return;

        if (isOverUI)
            return;

        Tower newTower = Instantiate(currentTower.Prefab, position, Quaternion.identity);
        OnTowerPlaced?.Invoke(newTower);
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
