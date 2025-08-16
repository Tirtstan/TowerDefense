using System;

[Serializable]
public class AudioSettings : IResetable
{
    public float MasterVolume;
    public float MusicVolume;
    public float SoundEffectsVolume;
    public float UserInterfaceVolume;

    public AudioSettings() => ResetToDefault();

    public void ResetToDefault()
    {
        MasterVolume = 0.5f;
        MusicVolume = 1f;
        SoundEffectsVolume = 1f;
        UserInterfaceVolume = 1f;
    }
}
