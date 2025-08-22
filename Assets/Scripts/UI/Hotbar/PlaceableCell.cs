using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaceableCell : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Image placeableIcon;

    [SerializeField]
    private TextMeshProUGUI costText;

    [SerializeField]
    private TowerSO towerSO;

    public void UpdateDisplay()
    {
        if (towerSO == null)
            return;

        placeableIcon.sprite = towerSO.Sprite;
        costText.SetText(towerSO.Cost.ToString());
    }

    private void OnValidate()
    {
        UpdateDisplay();
    }
}
