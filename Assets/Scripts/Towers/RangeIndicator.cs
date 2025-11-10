using UnityEngine;

public class RangeIndicator : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Tower tower;

    [SerializeField]
    private RectTransform rectTransform;

    private void OnEnable()
    {
        tower.OnUpgraded += OnTowerUpgraded;
        UpdateRangeIndicator();
    }

    private void UpdateRangeIndicator()
    {
        rectTransform.sizeDelta = tower.GetTowerSO().Stats.Range * 2 * Vector2.one;
    }

    private void OnTowerUpgraded(Tower tower) => UpdateRangeIndicator();

    private void OnDisable()
    {
        tower.OnUpgraded -= OnTowerUpgraded;
    }
}
