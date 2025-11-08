using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

// Anthropic, 2025
public class VHSPauseRenderPass : ScriptableRenderPass
{
    private const string PassName = "VHS Pause Effect";
    private Material material;

    private static readonly int GrainIntensityID = Shader.PropertyToID("_GrainIntensity");
    private static readonly int ScanlineIntensityID = Shader.PropertyToID("_ScanlineIntensity");
    private static readonly int VignetteIntensityID = Shader.PropertyToID("_VignetteIntensity");
    private static readonly int ChromaticAberrationID = Shader.PropertyToID("_ChromaticAberration");
    private static readonly int DesaturationID = Shader.PropertyToID("_Desaturation");
    private static readonly int MainTexID = Shader.PropertyToID("_MainTex");
    private static readonly int UnscaledTimeID = Shader.PropertyToID("_UnscaledTime");

    public VHSPauseRenderPass(Material material)
    {
        this.material = material;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    private class PassData
    {
        internal Material material;
        internal VHSPauseEffect effect;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var stack = VolumeManager.instance.stack;
        var vhsEffect = stack.GetComponent<VHSPauseEffect>();

        if (material == null || vhsEffect == null || !vhsEffect.IsActive())
            return;

        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData = frameData.Get<UniversalCameraData>();

        if (!resourceData.activeColorTexture.IsValid())
            return;

        // Create temp texture descriptor
        var descriptor = cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;

        var sourceTexture = resourceData.activeColorTexture;
        var tempTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, "_VHSPauseTemp", false);

        using (var builder = renderGraph.AddRasterRenderPass<PassData>(PassName, out var passData))
        {
            passData.material = material;
            passData.effect = vhsEffect;

            builder.UseTexture(sourceTexture, AccessFlags.Read);
            builder.SetRenderAttachment(tempTexture, 0, AccessFlags.Write);

            builder.SetRenderFunc(
                (PassData data, RasterGraphContext context) =>
                {
                    var effect = data.effect;

                    // Set the source texture
                    data.material.SetTexture(MainTexID, sourceTexture);

                    // Set unscaled time so the effect animates even when paused
                    data.material.SetFloat(UnscaledTimeID, Time.unscaledTime);

                    // Update material properties
                    data.material.SetFloat(GrainIntensityID, effect.grainIntensity.value * effect.intensity.value);
                    data.material.SetFloat(
                        ScanlineIntensityID,
                        effect.scanlineIntensity.value * effect.intensity.value
                    );
                    data.material.SetFloat(
                        VignetteIntensityID,
                        effect.vignetteIntensity.value * effect.intensity.value
                    );
                    data.material.SetFloat(
                        ChromaticAberrationID,
                        effect.chromaticAberration.value * effect.intensity.value
                    );
                    data.material.SetFloat(DesaturationID, effect.desaturation.value * effect.intensity.value);

                    // Blit with the VHS material
                    Blitter.BlitTexture(context.cmd, sourceTexture, new Vector4(1, 1, 0, 0), data.material, 0);
                }
            );
        }

        using (var builder = renderGraph.AddRasterRenderPass<PassData>("VHS Copy Back", out var passData))
        {
            passData.material = null;
            passData.effect = null;

            builder.UseTexture(tempTexture, AccessFlags.Read);
            builder.SetRenderAttachment(sourceTexture, 0, AccessFlags.Write);

            builder.SetRenderFunc(
                (PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, tempTexture, new Vector4(1, 1, 0, 0), 0, false);
                }
            );
        }
    }

    public void Dispose() { }
}


#region Reference List
/*

Anthropic. 2025. Claude Sonnet (Version 4.5). [Large language model]. Available at: https://claude.ai/ [Accessed: 07 November 2025].

*/
#endregion
