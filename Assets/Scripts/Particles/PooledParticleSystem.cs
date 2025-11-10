using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PooledParticleSystem : MonoBehaviour
{
    private ParticlePoolController particlePoolController;
    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    public void Initialize(ParticlePoolController controller)
    {
        particlePoolController = controller;
    }

    private void Update()
    {
        if (ps != null && particlePoolController != null && !ps.isPlaying && ps.particleCount == 0)
            particlePoolController.ReturnToPool(ps);
    }
}
