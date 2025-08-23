using System;
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
        TowerPlacer.OnTowerDeselected += OnTowerDeselected;
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
            TowerPlacer.Instance.SetCurrentTower(towerSO);
        else
            TowerPlacer.Instance.TryDeselectTower(towerSO);
    }

    public void UpdateDisplay()
    {
        if (towerSO == null)
            return;

        placeableIcon.sprite = towerSO.Sprite;
        costText.SetText(towerSO.Cost.ToString());
    }

    private void OnDestroy()
    {
        TowerPlacer.OnTowerDeselected -= OnTowerDeselected;
    }

    private void OnValidate() => UpdateDisplay();
}
