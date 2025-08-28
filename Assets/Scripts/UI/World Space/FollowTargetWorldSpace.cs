using UnityEngine;

public class FollowTargetWorldSpace : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Transform target;

    [Header("Configs")]
    [SerializeField]
    private Vector3 offset = Vector3.zero;

    [SerializeField]
    [Tooltip("Should the position be updated every frame?")]
    private bool updatePosition = true;

    private void OnEnable()
    {
        transform.position = GetTargetPosition();
    }

    private Vector3 GetTargetPosition()
    {
        if (target == null)
            return Vector3.zero;

        return target.position + offset;
    }

    private void Update()
    {
        if (updatePosition)
            transform.position = GetTargetPosition();
    }
}
