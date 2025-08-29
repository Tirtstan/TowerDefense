using UnityEngine;

public class UxPreviewController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private MeshRenderer previewRenderer;
    private MeshFilter previewFilter;

    private void Awake()
    {
        SelectionSystem.OnSelected += ShowPreview;
        SelectionSystem.OnDeselected += HidePreview;

        previewFilter = previewRenderer.GetComponent<MeshFilter>();
        previewRenderer.gameObject.SetActive(false);
    }

    private void ShowPreview(IGameSelectable selectable)
    {
        if (selectable == null || selectable.Transform == null)
            return;

        if (selectable.Transform.TryGetComponent(out TowerSelectable towerSelectable))
        {
            SetMesh(towerSelectable.GetMeshComponents().Item1, towerSelectable.GetMeshComponents().Item2);
            previewRenderer.gameObject.SetActive(true);
        }
    }

    private void HidePreview(IGameSelectable selectable)
    {
        previewRenderer.gameObject.SetActive(false);
    }

    private void SetMesh(MeshRenderer meshRenderer, MeshFilter meshFilter)
    {
        previewRenderer.sharedMaterial = meshRenderer.material;
        previewFilter.sharedMesh = meshFilter.mesh;
    }

    private void OnDestroy()
    {
        SelectionSystem.OnSelected -= ShowPreview;
        SelectionSystem.OnDeselected -= HidePreview;
    }
}
