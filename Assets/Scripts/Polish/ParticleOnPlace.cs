using UnityEngine;

public class ParticleOnPlace : MonoBehaviour
{
    [Header("Particle")]
    [SerializeField]
    private ParticleSystem placeParticleSystem;

    [SerializeField]
    private Vector3 offset = new(0, 0.5f, 0);

    private void Awake()
    {
        TowerPlacerController.OnTowerPlaced += HandleTowerPlaced;
    }

    private void HandleTowerPlaced(Tower tower)
    {
        placeParticleSystem.Stop();
        placeParticleSystem.transform.position = tower.transform.position + offset;
        placeParticleSystem.Play();
    }

    private void OnDestroy()
    {
        TowerPlacerController.OnTowerPlaced -= HandleTowerPlaced;
    }
}
