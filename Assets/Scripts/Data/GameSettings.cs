using System;

[Serializable]
public class GameSettings : IResetable
{
    public VideoSettings Video = new();
    public AudioSettings Audio = new();
    public ControlsSettings Controls = new();

    public GameSettings() => ResetToDefault();

    public void ResetToDefault()
    {
        Video.ResetToDefault();
        Audio.ResetToDefault();
        Controls.ResetToDefault();
    }
}
