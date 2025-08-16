using System;

[Serializable]
public class ControlsSettings : IResetable
{
    public float KeyNavSensitivity;
    public float MouseDragSensitivity;
    public float GamepadLookSensitivity;

    public ControlsSettings() => ResetToDefault();

    public void ResetToDefault()
    {
        KeyNavSensitivity = 1.5f;
        MouseDragSensitivity = 6f;
        GamepadLookSensitivity = 1.5f;
    }
}
