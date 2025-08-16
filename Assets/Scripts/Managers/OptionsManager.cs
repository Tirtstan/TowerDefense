using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class OptionsManager : Singleton<OptionsManager>
{
    [Header("Input Actions")]
    [SerializeField]
    private InputActionAsset inputActions;
    private const string IsSetUpKey = "IsSetUp";
    private const string GameSettingsKey = "GameSettings";
    private const string BindingsKey = "InputBindings";
    public GameSettings Settings { get; private set; } = new();

    private void Start()
    {
        if (PlayerPrefs.GetInt(IsSetUpKey, 1) == 1)
            ResetToDefault();
        else
            Load();

        ApplyAll();
    }

    public static int GetPlayerResolutionIndex()
    {
        Resolution[] resolutions = Screen.resolutions;
        Resolution currentRes = Screen.currentResolution;

        for (int i = 0; i < resolutions.Length; i++)
        {
            int result = resolutions[i].refreshRateRatio.value.CompareTo(currentRes.refreshRateRatio.value);
            if (resolutions[i].width == currentRes.width && resolutions[i].height == currentRes.height && result == 0)
                return i;
        }

        Debug.LogWarning("Could not find matching resolution. Returning 0...");
        return 0;
    }

    public void ResetToDefault()
    {
        Settings.ResetToDefault();
        Settings.Video.ResolutionIndex = GetPlayerResolutionIndex();
    }

    public void ApplyAll()
    {
        ApplyResolution();
        ApplyGraphics();
        ApplyAudio();

        Save();
    }

    private void ApplyResolution()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
            return;

        Resolution resolution = Screen.resolutions[Settings.Video.ResolutionIndex];
        Screen.SetResolution(
            resolution.width,
            resolution.height,
            Settings.Video.FullScreenMode,
            resolution.refreshRateRatio
        );
    }

    private void ApplyGraphics()
    {
        Application.targetFrameRate = Settings.Video.MaxFps;
        QualitySettings.vSyncCount = Settings.Video.VSyncCount;
    }

    private void ApplyAudio() { }

    private void Save()
    {
        PlayerPrefs.SetInt(IsSetUpKey, 0);

        PlayerPrefs.SetString(GameSettingsKey, JsonUtility.ToJson(Settings));
        PlayerPrefs.SetString(BindingsKey, inputActions.SaveBindingOverridesAsJson());

        PlayerPrefs.Save();
    }

    private void Load()
    {
        string json = PlayerPrefs.GetString(GameSettingsKey);
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                Settings = JsonUtility.FromJson<GameSettings>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load game settings: " + e.Message);
                ResetToDefault();
            }
        }
        else
        {
            ResetToDefault();
        }

        string rebinds = PlayerPrefs.GetString(BindingsKey);
        if (!string.IsNullOrEmpty(rebinds))
            inputActions.LoadBindingOverridesFromJson(rebinds);
    }
}
