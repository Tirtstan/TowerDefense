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
        MouseDragSensitivity = 4.5f;
        GamepadLookSensitivity = 1.5f;
    }
}
