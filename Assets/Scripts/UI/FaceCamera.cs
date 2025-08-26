using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        Vector3 directionToCamera = mainCamera.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(-directionToCamera);
    }
}
