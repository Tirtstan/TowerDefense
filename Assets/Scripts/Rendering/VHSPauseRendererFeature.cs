using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VHSPauseRendererFeature : ScriptableRendererFeature
{
    [SerializeField]
    private Shader vhsShader;

    private Material material;
    private VHSPauseRenderPass renderPass;

    public override void Create()
    {
        if (vhsShader == null)
        {
            vhsShader = Shader.Find("Hidden/VHSPauseEffect");
        }

        if (vhsShader == null)
        {
            Debug.LogError("VHS Pause Effect Shader not found!");
            return;
        }

        material = CoreUtils.CreateEngineMaterial(vhsShader);
        renderPass = new VHSPauseRenderPass(material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderPass == null || material == null)
            return;

        var stack = VolumeManager.instance.stack;
        var vhsEffect = stack.GetComponent<VHSPauseEffect>();

        if (vhsEffect != null && vhsEffect.IsActive())
            renderer.EnqueuePass(renderPass);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            renderPass?.Dispose();
            CoreUtils.Destroy(material);
        }
    }
}
