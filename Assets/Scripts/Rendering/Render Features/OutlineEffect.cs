using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineEffect : ScriptableRendererFeature
{

	[System.Serializable]
	public class OutlineEffectSettings
	{
		public Material effectMaterial = null;
	}

	public OutlineEffectSettings settings = new OutlineEffectSettings();

	private OutlineEffectPass _outlinePass;

	public override void Create()
	{
		// Pass erstellen, laueft zum Schluss
		_outlinePass = new OutlineEffectPass(settings.effectMaterial);
		_outlinePass.renderPassEvent = RenderPassEvent.AfterRendering;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		// Dem Renderer hinzufuegen
		renderer.EnqueuePass(_outlinePass);
	}
}

public class OutlineEffectPass : ScriptableRenderPass
{

	float[] _gaussKernel = {0.011254f, 0.016436f, 0.023066f, 0.031105f, 0.040306f, 0.050187f, 0.060049f,
													0.069041f, 0.076276f, 0.080977f, 0.082607f, 0.080977f, 0.076276f, 0.069041f,
													0.060049f, 0.050187f, 0.040306f, 0.031105f, 0.023066f, 0.016436f, 0.011254f};

	private Material _effectMaterial = null;

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		Camera usedCam = renderingData.cameraData.camera;
		// Buffer holen
		CommandBuffer cmdBuff = CommandBufferPool.Get();
		// Fullscreen Quad und inverse MVP erstellen
		Mesh fullQuad = RenderingUtils.fullscreenMesh;
		Matrix4x4 inverseVP = (GL.GetGPUProjectionMatrix(usedCam.projectionMatrix, false) * usedCam.worldToCameraMatrix).inverse;
		// Kernel speichern
		_effectMaterial.SetFloatArray("_kernel", _gaussKernel);
		_effectMaterial.SetInt("_kernelHalfWidth", _gaussKernel.Length / 2);
		// Draw Call hinzufuegen & ausfuehren
		cmdBuff.DrawMesh(fullQuad, inverseVP, _effectMaterial, 0, 1);
		context.ExecuteCommandBuffer(cmdBuff);
		// Buffer freigeben
		CommandBufferPool.Release(cmdBuff);
	}

	public OutlineEffectPass(Material effectMat)
	{
		_effectMaterial = effectMat;
	}
}
