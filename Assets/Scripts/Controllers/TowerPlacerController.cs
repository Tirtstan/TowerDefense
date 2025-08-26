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
    [Range(5f, 25f)]
    private float positionLerpSpeed = 15f;

    [SerializeField]
    private LayerMask checkLayer;

    [SerializeField]
    private LayerMask validLayer;

    [SerializeField]
    private LayerMask towerLayer;

    private PlayerInput playerInput;
    private TowerSO currentTowerSO;
    private Tower previewTower;
    private MeshRenderer previewTowerRenderer;
    private Camera mainCamera;
    private Vector2 mousePosition;
    private static bool isOverUI;
    private Material currentMaterial;
    private Vector3 targetPosition;

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

    private void FixedUpdate()
    {
        if (previewTower != null && previewTowerRenderer != null)
        {
            if (Raycast(out RaycastHit hit))
            {
                targetPosition = hit.point;
                bool canPlace = CanPlace(ref hit);

                Material targetMaterial = canPlace ? validMaterial : invalidMaterial;
                NTCursors targetCursor = canPlace ? placingCursor : invalidCursor;

                if (currentMaterial != targetMaterial)
                {
                    currentMaterial = targetMaterial;
                    previewTowerRenderer.material = currentMaterial;

                    CursorStack.Pop();
                    CursorStack.Push(targetCursor);
                }
            }

            previewTower.transform.position = Vector3.Lerp(
                previewTower.transform.position,
                targetPosition,
                positionLerpSpeed * Time.fixedDeltaTime
            );
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

    private bool HasEnoughCurrency() => EconomyManager.Instance.GetCurrencyAmount() >= currentTowerSO.Stats.Cost;

    private bool IsValidGround(ref RaycastHit hit)
    {
        if (hit.collider == null)
            return false;

        return (validLayer & (1 << hit.collider.gameObject.layer)) != 0;
    }

    private bool IsValidDistance()
    {
        Vector3 position = previewTower.transform.position;

        Collider[] results = new Collider[10];
        int nearbyTowerCount = Physics.OverlapSphereNonAlloc(position, minTowerDistance, results, towerLayer);

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
        previewTowerRenderer = null;
        currentMaterial = null;
        CursorStack.Clear();
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
        previewTower = Instantiate(
            currentTowerSO.Prefab,
            mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10f)),
            Quaternion.identity
        );
        previewTowerRenderer = previewTower.GetComponentInChildren<MeshRenderer>();
        OnTowerSelected?.Invoke(currentTowerSO);

        currentMaterial = null;
        CursorStack.Push(placingCursor);
    }

    public TowerSO GetCurrentTower() => currentTowerSO;

    public void PlaceTower(Vector3 position)
    {
        if (currentTowerSO == null)
            return;

        Tower placedTower = Instantiate(currentTowerSO.Prefab, position, Quaternion.identity);

        EconomyManager.Instance.RemoveCurrency(currentTowerSO.Stats.Cost);
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
