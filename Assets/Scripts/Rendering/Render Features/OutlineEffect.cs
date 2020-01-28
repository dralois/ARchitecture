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
	private static Mesh _FullscreenMesh = null;

	private static Mesh FullscreenMesh
	{
		get
		{
			if (_FullscreenMesh != null)
				return _FullscreenMesh;
			// Mesh erstellen
			_FullscreenMesh = new Mesh { name = "Fullscreen Quad" };
			// Vertices setzen
			_FullscreenMesh.vertices = new Vector3[]
			{
				new Vector3(0f, 0f, 0.1f),
				new Vector3(0f, 1f, 0.1f),
				new Vector3(1f, 1f, 0.1f),
				new Vector3(1f, 0f, 0.1f),
			};
			// UVs setzen
			_FullscreenMesh.uv = new Vector2[]
			{
				new Vector2(0f, 0f),
				new Vector2(0f, 1f),
				new Vector2(1f, 1f),
				new Vector2(1f, 0f),
			};
			// Upload
			_FullscreenMesh.SetIndices(new int[] { 0, 1, 2, 0, 2, 3 }, MeshTopology.Triangles, 0, false);
			_FullscreenMesh.UploadMeshData(true);
			return _FullscreenMesh;
		}
	}

	public override void Create()
	{
		// Pass erstellen, laueft zum Schluss
		_outlinePass = new OutlineEffectPass(settings.effectMaterial, FullscreenMesh);
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

	static readonly float[] _gaussKernel = {0.011254f, 0.016436f, 0.023066f, 0.031105f, 0.040306f, 0.050187f, 0.060049f,
																					0.069041f, 0.076276f, 0.080977f, 0.082607f, 0.080977f, 0.076276f, 0.069041f,
																					0.060049f, 0.050187f, 0.040306f, 0.031105f, 0.023066f, 0.016436f, 0.011254f};
	
	static readonly Matrix4x4 _BackgroundOrthoProjection = Matrix4x4.Ortho(0f, 1f, 0f, 1f, -0.1f, 9.9f);

	private Material _effectMaterial = null;
	private Mesh _fullQuadMesh = null;

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		Camera usedCam = renderingData.cameraData.camera;
		// Buffer holen
		CommandBuffer cmdBuff = CommandBufferPool.Get();
		cmdBuff.BeginSample("Outline Effect");
		// Kernel speichern
		_effectMaterial.SetFloatArray("_kernel", _gaussKernel);
		_effectMaterial.SetInt("_kernelHalfWidth", _gaussKernel.Length / 2);
		// Kamera auf Orthografisch umstellen
		cmdBuff.SetViewProjectionMatrices(Matrix4x4.identity, _BackgroundOrthoProjection);
		// Rendern
		cmdBuff.DrawMesh(_fullQuadMesh, Matrix4x4.identity, _effectMaterial, 0, 1);
		// Perspektivisch wiederherstellen
		cmdBuff.SetViewProjectionMatrices(usedCam.worldToCameraMatrix, usedCam.projectionMatrix);
		cmdBuff.EndSample("Outline Effect");
		// Ausfuehren
		context.ExecuteCommandBuffer(cmdBuff);
		// Buffer freigeben
		CommandBufferPool.Release(cmdBuff);
	}

	public OutlineEffectPass(Material effectMat, Mesh fullQuadMesh)
	{
		_effectMaterial = effectMat;
		_fullQuadMesh = fullQuadMesh;
	}
}
