using UnityEngine;

public class CenterTower : MonoBehaviour
{
    private void Start()
    {
        TowerManager.Instance.RegisterCenterTower(this);
    }

    public Vector3 GetPosition() => transform.position;
}
