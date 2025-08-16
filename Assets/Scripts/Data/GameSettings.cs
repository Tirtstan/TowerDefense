using System;

[Serializable]
public class GameSettings : IResetable
{
    public readonly VideoSettings Video = new();
    public readonly AudioSettings Audio = new();
    public readonly ControlsSettings Controls = new();

    public GameSettings() => ResetToDefault();

    public void ResetToDefault()
    {
        Video.ResetToDefault();
        Audio.ResetToDefault();
        Controls.ResetToDefault();
    }
}
