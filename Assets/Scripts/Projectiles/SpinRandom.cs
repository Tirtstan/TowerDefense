using UnityEngine;

public class SpinRandom : MonoBehaviour
{
    [Header("Configs")]
    [SerializeField]
    private Vector2 speedRange = new(1f, 5f);

    [SerializeField]
    private bool clockwise;

    [SerializeField]
    private bool randomiseDirection = true;
    private float spinSpeed;

    private void OnEnable()
    {
        float randomSpeed = Random.Range(speedRange.x, speedRange.y);

        int randomDirection = randomiseDirection ? (Random.value < 0.5f ? -1 : 1) : 1;
        if (!clockwise)
            randomDirection *= -1;

        spinSpeed = randomSpeed * randomDirection;
    }

    private void Update()
    {
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
    }
}
