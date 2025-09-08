using System.Collections.Generic;
using UnityEngine;

public class PreviewObject : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private LayerMask previewLayer;
    private readonly Dictionary<int, GameObject> previewCache = new();
    private GameObject currentPreviewInstance;

    private void Awake()
    {
        SelectionSystem.OnSelected += ShowPreview;
        SelectionSystem.OnDeselected += HidePreview;
    }

    private void ShowPreview(IGameSelectable selectable)
    {
        HideCurrentPreview();

        if (selectable.Transform.TryGetComponent(out Tower tower))
        {
            TowerSO towerSO = tower.GetTowerSO();
            if (!previewCache.TryGetValue(towerSO.Id, out currentPreviewInstance))
            {
                currentPreviewInstance = Instantiate(towerSO.OriginalMesh, transform);
                currentPreviewInstance.AddComponent<SpinObject>();
                SetAllLayers(currentPreviewInstance, Mathf.RoundToInt(Mathf.Log(previewLayer.value, 2)));
                previewCache[towerSO.Id] = currentPreviewInstance;
            }

            if (currentPreviewInstance != null)
                currentPreviewInstance.SetActive(true);
        }
    }

    private void HidePreview(IGameSelectable selectable) => HideCurrentPreview();

    private void HideCurrentPreview()
    {
        if (currentPreviewInstance != null)
        {
            currentPreviewInstance.SetActive(false);
            currentPreviewInstance = null;
        }
    }

    private void SetAllLayers(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetAllLayers(child.gameObject, layer);
    }

    private void OnDestroy()
    {
        SelectionSystem.OnSelected -= ShowPreview;
        SelectionSystem.OnDeselected -= HidePreview;
    }
}
