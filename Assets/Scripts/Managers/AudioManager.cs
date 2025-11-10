using System.Collections;
using System.Diagnostics;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource musicSource;

    [SerializeField]
    private AudioSource uiSource;

    [Header("Audio Mixer")]
    [SerializeField]
    private AudioMixerGroup musicMixerGroup;

    [SerializeField]
    private AudioMixerGroup sfxMixerGroup;

    [SerializeField]
    private AudioMixerGroup uiMixerGroup;

    [Header("Passes")]
    [SerializeField]
    private AudioHighPassFilter musicHighPassFilter;

    [SerializeField]
    private AudioLowPassFilter musicLowPassFilter;

    [Header("Pool Settings")]
    [SerializeField]
    private GameObject audioSourcePrefab;

    [SerializeField]
    [Range(5, 50)]
    private int poolDefaultCapacity = 10;

    [SerializeField]
    [Range(5, 50)]
    private int poolMaxSize = 30;

    [Header("Music Clips")]
    [SerializeField]
    private AudioClip mainMenuMusic;

    [SerializeField]
    private AudioClip gameplayMusic;

    [Header("UI Clips")]
    [SerializeField]
    private AudioClip[] selectClips;

    [SerializeField]
    private AudioClip[] menuClips;

    [Header("Pitches")]
    [SerializeField]
    private Vector2 sfxPitchRange = new(0.9f, 1.1f);

    [SerializeField]
    private Vector2 uiPitchRange = new(0.95f, 1.05f);

    private ObjectPool<AudioSource> sfxPool;
    private Tween musicFadeTween;
    private AudioClip lastPlayedClip;

    protected override void Awake()
    {
        base.Awake();
        InitializeAudioSources();
        InitializePool();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
            PlayMainMenuMusic();
        else
            PlayGameplayMusic();
    }

    private void InitializeAudioSources()
    {
        if (musicSource == null)
        {
            GameObject musicGO = new("MusicSource");
            musicGO.transform.SetParent(transform);
            musicSource = musicGO.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (uiSource == null)
        {
            GameObject uiGO = new("UISource");
            uiGO.transform.SetParent(transform);
            uiSource = uiGO.AddComponent<AudioSource>();
            uiSource.loop = false;
            uiSource.playOnAwake = false;
        }

        if (musicMixerGroup != null)
            musicSource.outputAudioMixerGroup = musicMixerGroup;
        if (uiMixerGroup != null)
            uiSource.outputAudioMixerGroup = uiMixerGroup;
    }

    private void InitializePool()
    {
        sfxPool = new ObjectPool<AudioSource>(
            createFunc: CreatePooledAudioSource,
            actionOnGet: OnGetFromPool,
            actionOnRelease: OnReturnToPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: true,
            defaultCapacity: poolDefaultCapacity,
            maxSize: poolMaxSize
        );
    }

    private AudioSource CreatePooledAudioSource()
    {
        GameObject go = Instantiate(audioSourcePrefab, transform);

        if (!go.TryGetComponent(out AudioSource audioSource))
            audioSource = go.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.outputAudioMixerGroup = sfxMixerGroup;

        if (!go.TryGetComponent(out PooledAudioSource pooledComponent))
            pooledComponent = go.AddComponent<PooledAudioSource>();

        pooledComponent.Initialize(this);
        return audioSource;
    }

    private void OnGetFromPool(AudioSource audioSource)
    {
        audioSource.gameObject.SetActive(true);
    }

    private void OnReturnToPool(AudioSource audioSource)
    {
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(AudioSource audioSource)
    {
        if (audioSource != null && audioSource.gameObject != null)
            Destroy(audioSource.gameObject);
    }

    public void ReturnToPool(AudioSource audioSource)
    {
        sfxPool.Release(audioSource);
    }

    public void PlayMusic(AudioClip clip, bool fadeIn = false, float fadeDuration = 1f)
    {
        if (clip == null)
            return;

        musicFadeTween?.Kill();

        if (fadeIn)
        {
            musicSource.clip = clip;
            musicSource.volume = 0f;
            musicSource.Play();
            musicFadeTween = musicSource.DOFade(1f, fadeDuration);
        }
        else
        {
            musicSource.clip = clip;
            musicSource.volume = 1f;
            musicSource.Play();
        }
    }

    public void ChangeMusic(AudioClip newClip, float fadeOutDuration = 1f, float fadeInDuration = 1f)
    {
        if (newClip == null || newClip == musicSource.clip)
            return;

        musicFadeTween?.Kill();

        musicFadeTween = musicSource
            .DOFade(0f, fadeOutDuration)
            .OnComplete(() =>
            {
                musicSource.clip = newClip;
                musicSource.Play();
                musicFadeTween = musicSource.DOFade(1f, fadeInDuration);
            });
    }

    public void StopMusic(bool fadeOut = false, float fadeDuration = 1f)
    {
        musicFadeTween?.Kill();

        if (fadeOut && musicSource.isPlaying)
        {
            float originalVolume = musicSource.volume;
            musicFadeTween = musicSource
                .DOFade(0f, fadeDuration)
                .OnComplete(() =>
                {
                    musicSource.Stop();
                    musicSource.volume = originalVolume;
                });
        }
        else
        {
            musicSource.Stop();
        }
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume);
    }

    public void PlayGameplayMusic(float maxVolume = 0.5f)
    {
        if (musicSource.clip == gameplayMusic)
            return;

        musicFadeTween?.Kill();
        musicSource.clip = gameplayMusic;
        musicSource.volume = 0f;
        musicSource.Play();
        musicFadeTween = musicSource.DOFade(maxVolume, 0.25f);
    }

    public void PlayMainMenuMusic()
    {
        if (musicSource.clip == mainMenuMusic)
            return;

        ChangeMusic(mainMenuMusic, fadeOutDuration: 1f, fadeInDuration: 1f);
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null)
            return;

        if (clip == lastPlayedClip)
            return;

        lastPlayedClip = clip;

        AudioSource audioSource = sfxPool.Get();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = Random.Range(sfxPitchRange.x, sfxPitchRange.y);
        audioSource.Play();

        StartCoroutine(ResetLastPlayedClip(audioSource.clip.length));
    }

    private IEnumerator ResetLastPlayedClip(float duration)
    {
        yield return new WaitForSeconds(duration);
        lastPlayedClip = null;
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (clip == null)
            return;

        AudioSource audioSource = sfxPool.Get();
        audioSource.transform.position = position;
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.spatialBlend = 1f;
        audioSource.Play();
    }

    public void PlayUI(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null)
            return;

        uiSource.pitch = pitch;
        uiSource.PlayOneShot(clip, volume);
    }

    public void PlayUIWithRandomPitch(AudioClip clip, float volume = 1f)
    {
        if (clip == null)
            return;

        uiSource.pitch = Random.Range(uiPitchRange.x, uiPitchRange.y);
        uiSource.PlayOneShot(clip, volume);
    }

    public void PlayRandomSelect(float volume = 1f, bool randomPitch = true) =>
        PlayFromRandomClips(selectClips, volume, randomPitch);

    public void PlayRandomMenu(float volume = 1f, bool randomPitch = true) =>
        PlayFromRandomClips(menuClips, volume, randomPitch);

    public void PlayFromRandomClips(AudioClip[] clips, float volume = 1f, bool randomPitch = true)
    {
        if (clips == null || clips.Length == 0)
            return;

        int randomIndex = Random.Range(0, clips.Length);
        if (!randomPitch)
            PlayUI(clips[randomIndex], volume);
        else
            PlayUIWithRandomPitch(clips[randomIndex], volume);
    }

    public AudioSource GetMusicSource() => musicSource;

    public void ToggleLowPassFilter(bool enable)
    {
        if (musicLowPassFilter == null)
            return;

        musicLowPassFilter.enabled = enable;
    }

    public void ToggleHighPassFilter(bool enable)
    {
        if (musicHighPassFilter == null)
            return;

        musicHighPassFilter.enabled = enable;
    }

    public void DisableAllFilters()
    {
        if (musicLowPassFilter != null)
            musicLowPassFilter.enabled = false;
        if (musicHighPassFilter != null)
            musicHighPassFilter.enabled = false;
    }

    private void OnDestroy()
    {
        musicFadeTween?.Kill();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
