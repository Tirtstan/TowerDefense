using TMPro;
using UnityEngine;

public class TowerInfo : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Tower tower;

    [SerializeField]
    private TextMeshProUGUI nameText;

    private void Awake()
    {
        nameText.SetText(tower.GetTowerSO().Name);
    }
}
