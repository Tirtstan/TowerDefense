using UnityEngine;

public class CenterTower : Singleton<CenterTower>
{
    public Vector3 GetPosition() => transform.position;
}
