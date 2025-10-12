using UnityEngine;

public class SpinObject : MonoBehaviour
{
    [Header("Configs")]
    [SerializeField]
    private Vector3 direction = Vector3.up;

    [SerializeField]
    private float spinSpeed = 10f;

    private void Update()
    {
        transform.Rotate(direction, spinSpeed * Time.deltaTime);
    }
}
