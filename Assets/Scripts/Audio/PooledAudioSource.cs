using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PooledAudioSource : MonoBehaviour
{
    private AudioManager audioManager;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Initialize(AudioManager manager)
    {
        audioManager = manager;
    }

    private void Update()
    {
        if (audioSource != null && !audioSource.isPlaying && audioSource.clip != null)
            audioManager.ReturnToPool(audioSource);
    }
}
