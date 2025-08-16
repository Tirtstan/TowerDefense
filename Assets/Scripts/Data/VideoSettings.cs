using System;
using UnityEngine;

[Serializable]
public class VideoSettings : IResetable
{
    public int ResolutionIndex;
    public FullScreenMode FullScreenMode;
    public int VSyncCount;
    public int MaxFps;
    public bool ShowFps;

    public VideoSettings() => ResetToDefault();

    public void ResetToDefault()
    {
        ResolutionIndex = 0;
        FullScreenMode = FullScreenMode.FullScreenWindow;
        VSyncCount = 1;
        MaxFps = -1;
        ShowFps = false;
    }
}
