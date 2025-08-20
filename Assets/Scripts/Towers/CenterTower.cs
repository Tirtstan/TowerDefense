using UnityEngine;

public class CenterTower : Singleton<CenterTower>
{
    protected override void Awake()
    {
        base.Awake();
    }

    public Vector3 GetPosition() => transform.position;
}
