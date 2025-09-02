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
        rectTransform.sizeDelta = tower.GetTowerSO().Stats.Range * 2 * Vector2.one;
    }
}
