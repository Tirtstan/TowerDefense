using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable, VolumeComponentMenu("Post-processing/Custom/VHS Pause")]
public class VHSPauseEffect : VolumeComponent, IPostProcessComponent
{
    [Header("VHS Effect Settings")]
    [Tooltip("Overall intensity of the VHS effect")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("Film grain intensity")]
    public ClampedFloatParameter grainIntensity = new ClampedFloatParameter(0.1f, 0f, 1f);

    [Tooltip("Scanline intensity")]
    public ClampedFloatParameter scanlineIntensity = new ClampedFloatParameter(0.05f, 0f, 1f);

    [Tooltip("Vignette intensity")]
    public ClampedFloatParameter vignetteIntensity = new ClampedFloatParameter(0.3f, 0f, 1f);

    [Tooltip("Chromatic aberration intensity")]
    public ClampedFloatParameter chromaticAberration = new ClampedFloatParameter(0.002f, 0f, 0.01f);

    [Tooltip("Desaturation amount")]
    public ClampedFloatParameter desaturation = new ClampedFloatParameter(0.3f, 0f, 1f);

    public bool IsActive() => intensity.value > 0.01f;

    public bool IsTileCompatible() => false;
}
