using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaceableCell : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Toggle toggle;

    [SerializeField]
    private Image placeableIcon;

    [SerializeField]
    private TextMeshProUGUI costText;

    [SerializeField]
    private TowerSO towerSO;

    private void Awake()
    {
        TowerPlacerController.OnTowerDeselected += OnTowerDeselected;
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
        UpdateDisplay();
    }

    private void OnTowerDeselected(TowerSO tower)
    {
        if (tower == towerSO)
            toggle.isOn = false;
    }

    private void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
            TowerPlacerController.Instance.SetCurrentTower(towerSO);
        else
            TowerPlacerController.Instance.TryDeselectTower(towerSO);
    }

    public void UpdateDisplay()
    {
        if (towerSO == null)
            return;

        placeableIcon.sprite = towerSO.Sprite;
        costText.SetText(towerSO.Stats.Cost.ToString());
    }

    private void OnDestroy()
    {
        TowerPlacerController.OnTowerDeselected -= OnTowerDeselected;
    }

    private void OnValidate() => UpdateDisplay();
}
