using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullscreenEffect : ScriptableRendererFeature
{

	[System.Serializable]
	public class FullscreenEffectSettings
	{
		public Material effectMaterial = null;
	}

	public FullscreenEffectSettings settings = new FullscreenEffectSettings();

	private FullscreenEffectPass _scriptablePass;

	public override void Create()
	{
		// Pass erstellen, laueft zum Schluss
		_scriptablePass = new FullscreenEffectPass(settings.effectMaterial);
		_scriptablePass.renderPassEvent = RenderPassEvent.AfterRendering;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		// Dem Renderer hinzufuegen
		renderer.EnqueuePass(_scriptablePass);
	}
}

public class FullscreenEffectPass : ScriptableRenderPass
{

	private Material _effectMaterial = null;

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		Camera usedCam = renderingData.cameraData.camera;
		// Buffer holen
		CommandBuffer cmdBuff = CommandBufferPool.Get();
		cmdBuff.BeginSample("Fullscreen Effect");
		// Fullscreen Quad und inverse MVP erstellen
		Mesh fullQuad = RenderingUtils.fullscreenMesh;
		Matrix4x4 inverseVP = (GL.GetGPUProjectionMatrix(usedCam.projectionMatrix, false) * usedCam.worldToCameraMatrix).inverse;
		// Draw Call hinzufuegen & ausfuehren
		cmdBuff.DrawMesh(fullQuad, inverseVP, _effectMaterial);
		cmdBuff.EndSample("Fullscreen Effect");
		context.ExecuteCommandBuffer(cmdBuff);
		// Buffer freigeben
		CommandBufferPool.Release(cmdBuff);
	}

	public FullscreenEffectPass(Material effectMat)
	{
		_effectMaterial = effectMat;
	}
}
