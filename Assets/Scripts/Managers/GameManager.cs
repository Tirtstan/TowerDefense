using UnityEngine;

[DefaultExecutionOrder(-10)]
public class GameManager : Singleton<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
        Random.InitState((int)System.DateTime.Now.Ticks);
    }
}
