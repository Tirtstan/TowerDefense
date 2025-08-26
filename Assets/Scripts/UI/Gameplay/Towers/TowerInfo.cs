using TMPro;
using UnityEngine;

public class TowerInfo : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private TowerSO towerSO;

    [SerializeField]
    private TextMeshProUGUI nameText;

    private void Awake()
    {
        nameText.SetText(towerSO.Name);
    }
}
