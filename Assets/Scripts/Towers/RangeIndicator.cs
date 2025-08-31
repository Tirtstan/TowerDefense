using UnityEngine;

public class RangeIndicator : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private TowerSO towerSO;

    [SerializeField]
    private RectTransform rectTransform;

    private void OnEnable()
    {
        rectTransform.sizeDelta = towerSO.Stats.Range * 2 * Vector2.one;
    }
}
