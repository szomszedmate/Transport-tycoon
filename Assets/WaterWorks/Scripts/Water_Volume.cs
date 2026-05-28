using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class Water_Volume : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        private Material _material;
        private const string ProfilerTag = "Water Volume Pass";

        public CustomRenderPass(Material mat)
        {
            _material = mat;
        }

        private class PassData
        {
            public Material material;
            public TextureHandle source;
            public TextureHandle temp;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_material == null) return;

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            if (cameraData.cameraType == CameraType.Reflection) return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            TextureHandle source = resourceData.activeColorTexture;

            RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.colorFormat = RenderTextureFormat.Default;

            TextureHandle temp = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                descriptor,
                "_TemporaryColourTexture",
                false,
                FilterMode.Bilinear
            );

            using (var builder = renderGraph.AddUnsafePass<PassData>(ProfilerTag, out var passData))
            {
                passData.material = _material;
                passData.source = source;
                passData.temp = temp;

                builder.UseTexture(source, AccessFlags.ReadWrite);
                builder.UseTexture(temp, AccessFlags.ReadWrite);

                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) =>
                {
                    CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
                    Blitter.BlitCameraTexture(cmd, data.source, data.temp, data.material, 0);
                    Blitter.BlitCameraTexture(cmd, data.temp, data.temp, 1.0f);
                });
            }
        }

        public override void OnCameraCleanup(CommandBuffer cmd) { }
    }

    [System.Serializable]
    public class _Settings
    {
        public Material material = null;
        public RenderPassEvent renderPass = RenderPassEvent.AfterRenderingSkybox;
    }

    public _Settings settings = new _Settings();
    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        if (settings.material == null)
        {
            settings.material = (Material)Resources.Load("Water_Volume");
        }
        m_ScriptablePass = new CustomRenderPass(settings.material);
        m_ScriptablePass.renderPassEvent = settings.renderPass;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}