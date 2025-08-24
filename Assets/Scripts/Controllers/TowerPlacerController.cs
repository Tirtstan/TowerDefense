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

    [Header("Materials")]
    [SerializeField]
    private Material validMaterial;

    [SerializeField]
    private Material invalidMaterial;

    [Header("Cursors")]
    [SerializeField]
    private NTCursors placingCursor = NTCursors.ResizeVertical;

    [SerializeField]
    private NTCursors invalidCursor = NTCursors.Invalid;

    [Header("Configs")]
    [SerializeField]
    private float maxPlacementDistance = 50f;

    [SerializeField]
    [Range(0f, 5f)]
    private float minTowerDistance = 1f;

    [SerializeField]
    private LayerMask checkLayer;

    [SerializeField]
    private LayerMask validLayer;

    [SerializeField]
    private LayerMask towerLayer;
    private PlayerInput playerInput;
    private int cursorId;
    private TowerSO currentTowerSO;
    private Tower previewTower;
    private MeshRenderer previewTowerRenderer;
    private Camera mainCamera;
    private Vector2 mousePosition;
    private static bool isOverUI;
    private bool isCurrentlyValid;

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
            {
                previewTower.transform.position = hit.point;
                bool canPlace = CanPlace(ref hit);

                if (canPlace != isCurrentlyValid) // prevents it from spamming duplicate changes
                {
                    isCurrentlyValid = canPlace;
                    if (canPlace)
                    {
                        previewTowerRenderer.material = validMaterial;
                        CursorStack.Pop();
                        CursorStack.Push(placingCursor);
                    }
                    else
                    {
                        previewTowerRenderer.material = invalidMaterial;
                        CursorStack.Pop();
                        CursorStack.Push(invalidCursor);
                    }
                }
            }
        }
    }

    private void OnPlacePerformed(InputAction.CallbackContext context)
    {
        if (currentTowerSO == null)
            return;

        if (Raycast(out RaycastHit hit) && CanPlace(ref hit))
            PlaceTower(hit.point);
    }

    private bool Raycast(out RaycastHit hit)
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        return Physics.Raycast(ray, out hit, maxPlacementDistance, checkLayer);
    }

    private bool CanPlace(ref RaycastHit hit)
    {
        if (currentTowerSO == null || previewTower == null)
            return false;

        if (!HasEnoughCurrency())
            return false;

        if (isOverUI)
            return false;

        if (!IsValidGround(ref hit))
            return false;

        if (!IsValidDistance())
            return false;

        return true;
    }

    private bool HasEnoughCurrency() => EconomyManager.Instance.GetCurrencyAmount() >= currentTowerSO.Cost;

    private bool IsValidGround(ref RaycastHit hit)
    {
        if (hit.collider == null)
            return false;

        return (validLayer & (1 << hit.collider.gameObject.layer)) != 0;
    }

    private bool IsValidDistance()
    {
        Vector3 position = previewTower.transform.position;

        // Check for overlapping towers within minimum distance
        Collider[] results = new Collider[10];
        int nearbyTowerCount = Physics.OverlapSphereNonAlloc(position, minTowerDistance, results, towerLayer);

        // Filter out the preview tower itself (if it has a collider)
        for (int i = 0; i < nearbyTowerCount; i++)
        {
            Collider tower = results[i];
            if (tower.gameObject != previewTower.gameObject)
                return false;
        }

        return true;
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
        previewTowerRenderer = previewTower.GetComponentInChildren<MeshRenderer>(); // move this to own class on obj
        OnTowerSelected?.Invoke(currentTowerSO);

        cursorId = CursorStack.Push(placingCursor);
    }

    public TowerSO GetCurrentTower() => currentTowerSO;

    public void PlaceTower(Vector3 position)
    {
        if (currentTowerSO == null)
            return;

        Tower placedTower = Instantiate(currentTowerSO.Prefab);
        placedTower.transform.position = position;

        EconomyManager.Instance.RemoveCurrency(currentTowerSO.Cost);
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
