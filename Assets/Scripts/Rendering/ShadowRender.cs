using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ShadowRender : ScriptableRendererFeature
{

	[System.Serializable]
	public class ShadowPassSettings
	{
		public Material shadowMaterial = null;
	}

	public ShadowPassSettings settings = new ShadowPassSettings();

	private ShadowRenderPass _scriptablePass;

	public override void Create()
	{
		// Pass erstellen, laueft zum Schluss
		_scriptablePass = new ShadowRenderPass(settings.shadowMaterial);
		_scriptablePass.renderPassEvent = RenderPassEvent.AfterRendering;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		// Dem Renderer hinzufuegen
		renderer.EnqueuePass(_scriptablePass);
	}
}

public class ShadowRenderPass : ScriptableRenderPass
{

	private Material _shadowMaterial = null;

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		Camera usedCam = renderingData.cameraData.camera;
		// Buffer holen
		CommandBuffer cmdBuff = CommandBufferPool.Get();
		// Fullscreen Quad und inverse MVP erstellen
		Mesh fullQuad = RenderingUtils.fullscreenMesh;
		Matrix4x4 inverseVP = (GL.GetGPUProjectionMatrix(usedCam.projectionMatrix, false) * usedCam.worldToCameraMatrix).inverse;
		// Draw Call hinzufuegen & ausfuehren
		cmdBuff.DrawMesh(fullQuad, inverseVP, _shadowMaterial);
		context.ExecuteCommandBuffer(cmdBuff);
		// Buffer freigeben
		CommandBufferPool.Release(cmdBuff);
	}

	public ShadowRenderPass(Material shadowMaterial)
	{
		_shadowMaterial = shadowMaterial;
	}
}
