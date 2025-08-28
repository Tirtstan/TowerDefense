using UnityEngine;

public class SpinObject : MonoBehaviour
{
    [Header("Configs")]
    [SerializeField]
    private float spinSpeed = 20f;

    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }
}
