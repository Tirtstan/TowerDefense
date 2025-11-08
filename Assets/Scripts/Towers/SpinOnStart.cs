using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

public class SpinOnStart : MonoBehaviour
{
    [Header("Configs")]
    [SerializeField]
    private Transform target;

    [SerializeField]
    private SerializableMotionSettings<Vector3, NoOptions> spinSettings;

    private void Start()
    {
        LMotion.Create(spinSettings).BindToEulerAngles(target).AddTo(gameObject);
    }
}
