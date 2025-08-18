using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[CustomEditor(typeof(OptionsManager))]
public class OptionsManagerEditor : Editor
{
    private OptionsManager optionsManager;
    private SerializedProperty inputActionsProperty;

    private bool videoFoldout = true;
    private bool audioFoldout = true;
    private bool controlsFoldout = true;
    private bool inputBindingsFoldout = false;
    private bool debugFoldout = false;
    private string[] resolutionOptions;
    private Resolution[] availableResolutions;

    private void OnEnable()
    {
        optionsManager = (OptionsManager)target;
        inputActionsProperty = serializedObject.FindProperty("inputActions");

        RefreshResolutionData();
    }

    private void RefreshResolutionData()
    {
        availableResolutions = Screen.resolutions;
        resolutionOptions = new string[availableResolutions.Length];

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            Resolution res = availableResolutions[i];
            resolutionOptions[i] = $"{res.width}x{res.height} @ {res.refreshRateRatio.value:F0}Hz";
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(10);

        GUIStyle headerStyle = new(EditorStyles.boldLabel) { fontSize = 16, alignment = TextAnchor.MiddleCenter };

        EditorGUILayout.LabelField("Options Manager", headerStyle);
        EditorGUILayout.Space(5);

        EditorGUILayout.PropertyField(inputActionsProperty, new GUIContent("Input Actions Asset"));
        EditorGUILayout.Space(10);

        DrawMainActionButtons();
        EditorGUILayout.Space(10);

        DrawVideoSettings();
        EditorGUILayout.Space(5);

        DrawAudioSettings();
        EditorGUILayout.Space(5);

        DrawControlsSettings();
        EditorGUILayout.Space(5);

        DrawInputBindingsSection();
        EditorGUILayout.Space(5);

        DrawDebugSection();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed && Application.isPlaying)
            EditorUtility.SetDirty(optionsManager);
    }

    private void DrawMainActionButtons()
    {
        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Apply All Settings", GUILayout.Height(30)))
        {
            if (Application.isPlaying)
            {
                optionsManager.ApplyAll();
                EditorUtility.DisplayDialog("Settings Applied", "All settings have been applied successfully!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Play Mode Required",
                    "Settings can only be applied during play mode.",
                    "OK"
                );
            }
        }

        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("Reset to Defaults", GUILayout.Height(30)))
        {
            if (
                EditorUtility.DisplayDialog(
                    "Reset Settings",
                    "Are you sure you want to reset all settings to their default values?",
                    "Yes",
                    "Cancel"
                )
            )
            {
                optionsManager.ResetToDefault();
                EditorUtility.SetDirty(optionsManager);
            }
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
    }

    private void DrawVideoSettings()
    {
        videoFoldout = EditorGUILayout.Foldout(videoFoldout, "Video Settings", true, EditorStyles.foldoutHeader);

        if (videoFoldout)
        {
            EditorGUI.indentLevel++;

            VideoSettings videoSettings = optionsManager.Settings.Video;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Resolution", GUILayout.Width(150));

            int newResolutionIndex = EditorGUILayout.Popup(videoSettings.ResolutionIndex, resolutionOptions);
            if (newResolutionIndex != videoSettings.ResolutionIndex)
            {
                videoSettings.ResolutionIndex = newResolutionIndex;
                EditorUtility.SetDirty(optionsManager);
            }

            if (GUILayout.Button("Refresh", GUILayout.Width(60)))
                RefreshResolutionData();

            EditorGUILayout.EndHorizontal();

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    $"Current: {Screen.currentResolution.width}x{Screen.currentResolution.height} @ {Screen.currentResolution.refreshRateRatio.value:F0}Hz",
                    MessageType.Info
                );
            }

            var newFullScreenMode = (FullScreenMode)
                EditorGUILayout.EnumPopup("Fullscreen Mode", videoSettings.FullScreenMode);
            if (newFullScreenMode != videoSettings.FullScreenMode)
            {
                videoSettings.FullScreenMode = newFullScreenMode;
                EditorUtility.SetDirty(optionsManager);
            }

            string[] vSyncOptions = { "Disabled", "Every V Blank", "Every Second V Blank" };
            int newVSyncCount = EditorGUILayout.Popup("VSync", videoSettings.VSyncCount, vSyncOptions);
            if (newVSyncCount != videoSettings.VSyncCount)
            {
                videoSettings.VSyncCount = newVSyncCount;
                EditorUtility.SetDirty(optionsManager);
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max FPS", GUILayout.Width(150));

            if (videoSettings.MaxFps == -1)
            {
                EditorGUILayout.LabelField("Unlimited");
            }
            else
            {
                EditorGUILayout.LabelField(videoSettings.MaxFps.ToString());
            }

            if (GUILayout.Button("30", GUILayout.Width(40)))
            {
                videoSettings.MaxFps = 30;
                EditorUtility.SetDirty(optionsManager);
            }
            if (GUILayout.Button("60", GUILayout.Width(40)))
            {
                videoSettings.MaxFps = 60;
                EditorUtility.SetDirty(optionsManager);
            }
            if (GUILayout.Button("120", GUILayout.Width(40)))
            {
                videoSettings.MaxFps = 120;
                EditorUtility.SetDirty(optionsManager);
            }
            if (GUILayout.Button("Unlimited", GUILayout.Width(70)))
            {
                videoSettings.MaxFps = -1;
                EditorUtility.SetDirty(optionsManager);
            }

            EditorGUILayout.EndHorizontal();

            bool newShowFps = EditorGUILayout.Toggle("Show FPS", videoSettings.ShowFps);
            if (newShowFps != videoSettings.ShowFps)
            {
                videoSettings.ShowFps = newShowFps;
                EditorUtility.SetDirty(optionsManager);
            }

            EditorGUI.indentLevel--;
        }
    }

    private void DrawAudioSettings()
    {
        audioFoldout = EditorGUILayout.Foldout(audioFoldout, "Audio Settings", true, EditorStyles.foldoutHeader);

        if (audioFoldout)
        {
            EditorGUI.indentLevel++;

            var audioSettings = optionsManager.Settings.Audio;

            float newMasterVolume = EditorGUILayout.Slider("Master Volume", audioSettings.MasterVolume, 0f, 1f);
            if (!Mathf.Approximately(newMasterVolume, audioSettings.MasterVolume))
            {
                audioSettings.MasterVolume = newMasterVolume;
                EditorUtility.SetDirty(optionsManager);
            }

            float newMusicVolume = EditorGUILayout.Slider("Music Volume", audioSettings.MusicVolume, 0f, 1f);
            if (!Mathf.Approximately(newMusicVolume, audioSettings.MusicVolume))
            {
                audioSettings.MusicVolume = newMusicVolume;
                EditorUtility.SetDirty(optionsManager);
            }

            float newSfxVolume = EditorGUILayout.Slider(
                "Sound Effects Volume",
                audioSettings.SoundEffectsVolume,
                0f,
                1f
            );
            if (!Mathf.Approximately(newSfxVolume, audioSettings.SoundEffectsVolume))
            {
                audioSettings.SoundEffectsVolume = newSfxVolume;
                EditorUtility.SetDirty(optionsManager);
            }

            float newUiVolume = EditorGUILayout.Slider("UI Volume", audioSettings.UserInterfaceVolume, 0f, 1f);
            if (!Mathf.Approximately(newUiVolume, audioSettings.UserInterfaceVolume))
            {
                audioSettings.UserInterfaceVolume = newUiVolume;
                EditorUtility.SetDirty(optionsManager);
            }

            EditorGUI.indentLevel--;
        }
    }

    private void DrawControlsSettings()
    {
        controlsFoldout = EditorGUILayout.Foldout(
            controlsFoldout,
            "Controls Settings",
            true,
            EditorStyles.foldoutHeader
        );

        if (controlsFoldout)
        {
            EditorGUI.indentLevel++;

            var controlsSettings = optionsManager.Settings.Controls;

            float newKeyNavSensitivity = EditorGUILayout.Slider(
                "Key Navigation Sensitivity",
                controlsSettings.KeyNavSensitivity,
                0.1f,
                5f
            );
            if (!Mathf.Approximately(newKeyNavSensitivity, controlsSettings.KeyNavSensitivity))
            {
                controlsSettings.KeyNavSensitivity = newKeyNavSensitivity;
                EditorUtility.SetDirty(optionsManager);
            }

            float newMouseDragSensitivity = EditorGUILayout.Slider(
                "Mouse Drag Sensitivity",
                controlsSettings.MouseDragSensitivity,
                0.1f,
                10f
            );
            if (!Mathf.Approximately(newMouseDragSensitivity, controlsSettings.MouseDragSensitivity))
            {
                controlsSettings.MouseDragSensitivity = newMouseDragSensitivity;
                EditorUtility.SetDirty(optionsManager);
            }

            float newGamepadLookSensitivity = EditorGUILayout.Slider(
                "Gamepad Look Sensitivity",
                controlsSettings.GamepadLookSensitivity,
                0.1f,
                5f
            );
            if (!Mathf.Approximately(newGamepadLookSensitivity, controlsSettings.GamepadLookSensitivity))
            {
                controlsSettings.GamepadLookSensitivity = newGamepadLookSensitivity;
                EditorUtility.SetDirty(optionsManager);
            }

            EditorGUI.indentLevel--;
        }
    }

    private void DrawInputBindingsSection()
    {
        inputBindingsFoldout = EditorGUILayout.Foldout(
            inputBindingsFoldout,
            "Input Bindings",
            true,
            EditorStyles.foldoutHeader
        );

        if (inputBindingsFoldout)
        {
            EditorGUI.indentLevel++;

            if (inputActionsProperty.objectReferenceValue != null)
            {
                var inputActions = inputActionsProperty.objectReferenceValue as InputActionAsset;

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset All Bindings"))
                {
                    if (
                        EditorUtility.DisplayDialog(
                            "Reset Bindings",
                            "Are you sure you want to reset all input bindings to their defaults?",
                            "Yes",
                            "Cancel"
                        )
                    )
                    {
                        inputActions.RemoveAllBindingOverrides();
                        EditorUtility.SetDirty(inputActions);
                    }
                }

                if (GUILayout.Button("Open Input Actions"))
                {
                    Selection.activeObject = inputActions;
                    EditorGUIUtility.PingObject(inputActions);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.HelpBox(
                    "To modify input bindings, select the Input Actions asset above or use the runtime rebinding system in-game.",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox("No Input Actions asset assigned!", MessageType.Warning);
            }

            EditorGUI.indentLevel--;
        }
    }

    private void DrawDebugSection()
    {
        debugFoldout = EditorGUILayout.Foldout(debugFoldout, "Debug Info", true, EditorStyles.foldoutHeader);

        if (debugFoldout)
        {
            EditorGUI.indentLevel++;

            GUI.enabled = false;

            EditorGUILayout.LabelField("PlayerPrefs Keys:", EditorStyles.boldLabel);
            EditorGUILayout.TextField("Setup Key", "IsSetUp");
            EditorGUILayout.TextField("Settings Key", "GameSettings");
            EditorGUILayout.TextField("Bindings Key", "InputBindings");

            EditorGUILayout.Space(5);

            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Runtime Info:", EditorStyles.boldLabel);
                EditorGUILayout.TextField(
                    "Current Resolution",
                    $"{Screen.currentResolution.width}x{Screen.currentResolution.height}"
                );
                EditorGUILayout.TextField("Target Frame Rate", Application.targetFrameRate.ToString());
                EditorGUILayout.TextField("VSync Count", QualitySettings.vSyncCount.ToString());
            }

            GUI.enabled = true;

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear PlayerPrefs"))
            {
                if (
                    EditorUtility.DisplayDialog(
                        "Clear PlayerPrefs",
                        "This will delete all saved settings. Are you sure?",
                        "Yes",
                        "Cancel"
                    )
                )
                {
                    PlayerPrefs.DeleteKey("IsSetUp");
                    PlayerPrefs.DeleteKey("GameSettings");
                    PlayerPrefs.DeleteKey("InputBindings");
                    PlayerPrefs.Save();
                    EditorUtility.DisplayDialog("PlayerPrefs Cleared", "All PlayerPrefs have been cleared.", "OK");
                }
            }

            if (GUILayout.Button("Force Save"))
            {
                if (Application.isPlaying)
                {
                    var saveMethod = typeof(OptionsManager).GetMethod(
                        "Save",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
                    );
                    saveMethod?.Invoke(optionsManager, null);
                    EditorUtility.DisplayDialog("Settings Saved", "Settings have been saved to PlayerPrefs.", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Play Mode Required",
                        "Settings can only be saved during play mode.",
                        "OK"
                    );
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }
    }
}
